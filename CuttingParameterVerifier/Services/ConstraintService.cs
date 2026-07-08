using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using CuttingParameterVerifier.Models;

namespace CuttingParameterVerifier.Services;

public sealed class ConstraintService : IConstraintService
{
    internal const string BundledResourceLogicalName = "BundledConstraints.json";

    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ConstraintService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>Deserialize once — never mutated; used only to augment disk config.</summary>
    private static readonly Lazy<VerificationConfig?> BundledBaseline = new(() => DeserializeBundledFromExecutingAssembly(JsonOptions));

    public event Action? ConfigurationChanged;

    public ConstraintService(IWebHostEnvironment env, ILogger<ConstraintService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public VerificationConfig Load()
    {
        var path = GetConfigPath();

        VerificationConfig cfg;
        try
        {
            if (!File.Exists(path))
            {
                cfg = SnapshotBundledBaseline() ?? DefaultVerificationConfigFactory.Create();
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, JsonSerializer.Serialize(cfg, JsonOptions));
                _logger.LogInformation("Created initial constraints at {Path}", path);
                return cfg;
            }

            var json = File.ReadAllText(path);
            cfg = JsonSerializer.Deserialize<VerificationConfig>(json, JsonOptions) ?? SnapshotBundledBaseline() ?? DefaultVerificationConfigFactory.Create();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load constraints file; rebuilding from bundled or defaults.");
            cfg = SnapshotBundledBaseline() ?? DefaultVerificationConfigFactory.Create();
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, JsonSerializer.Serialize(cfg, JsonOptions));
            }
            catch (Exception inner)
            {
                _logger.LogWarning(inner, "Could not persist recovered constraints.");
            }
        }

        var bundled = BundledBaseline.Value;
        if (bundled is not null)
        {
            var merged = MergeMissingMappingsAndGraphs(cfg, bundled);
            if (merged.RuleCount > 0 || merged.GraphCount > 0)
            {
                _logger.LogWarning(
                    "Augmented persisted constraints.json with missing bundled mappings ({Rules} rule(s)) and graphs ({Graphs} graph(s)); saving updated config. This usually means an older constraints file existed on hosting.",
                    merged.RuleCount,
                    merged.GraphCount);
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                    File.WriteAllText(path, JsonSerializer.Serialize(cfg, JsonOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Could not save augmented constraints to {Path}. In-memory config is still complete for this process, but merges will repeat until the file becomes writable.",
                        path);
                }
            }
        }

        foreach (var g in cfg.Graphs)
        {
            if (g.EngagementMode == EngagementMode.DiameterScaled)
                DiameterInequalityService.EnsureInequalities(g);
        }

        return cfg;
    }

    public void Save(VerificationConfig config)
    {
        var path = GetConfigPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(config, JsonOptions));
        ConfigurationChanged?.Invoke();
    }

    public ConstraintGraph? FindGraph(VerificationConfig config, string graphNumber)
    {
        var key = graphNumber.Trim();
        return config.Graphs.FirstOrDefault(g => string.Equals(g.GraphNumber.Trim(), key, StringComparison.Ordinal));
    }

    private string GetConfigPath() => Path.Combine(_env.ContentRootPath, "Data", "constraints.json");

    /// <summary>Detached copy via JSON round-trip — safe to write to disk without sharing references with <see cref="BundledBaseline"/>.</summary>
    private static VerificationConfig? SnapshotBundledBaseline()
    {
        var bundled = BundledBaseline.Value;
        if (bundled is null)
            return null;
        try
        {
            return JsonSerializer.Deserialize<VerificationConfig>(JsonSerializer.Serialize(bundled, JsonOptions), JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static VerificationConfig? DeserializeBundledFromExecutingAssembly(JsonSerializerOptions options)
    {
        var asm = Assembly.GetExecutingAssembly();
        using var s = asm.GetManifestResourceStream(BundledResourceLogicalName);
        if (s is null)
            return null;
        try
        {
            return JsonSerializer.Deserialize<VerificationConfig>(s, options);
        }
        catch
        {
            return null;
        }
    }

    private sealed record MergeResult(int RuleCount, int GraphCount);

    private MergeResult MergeMissingMappingsAndGraphs(VerificationConfig disk, VerificationConfig bundled)
    {
        // Graphs first so new upgrade mappings can resolve their targets on disk.
        var addedGraphs = 0;
        foreach (var g in bundled.Graphs)
        {
            if (disk.Graphs.Any(x => SameGraphNumber(x.GraphNumber, g.GraphNumber)))
                continue;
            // Superseded by rename: disk maps this key to a figure that is not in the bundled
            // library (renamed away). Do not re-add the old id next to the user's rename.
            // Multi-figure keys (several bundled graphs sharing a mapping key) are unaffected
            // when disk still points at other bundled figure ids.
            if (IsBundledGraphSupersededByRename(disk, bundled, g.GraphNumber))
                continue;
            disk.Graphs.Add(CloneGraph(g));
            addedGraphs++;
        }

        var addedRules = 0;
        foreach (var b in bundled.MappingRules)
        {
            if (HasEquivalentMapping(disk.MappingRules, b))
                continue;
            // Skip mappings whose target graph was renamed or deleted on disk — otherwise Save→Load
            // would resurrect the old graph number from the embedded baseline.
            if (!disk.Graphs.Any(g => SameGraphNumber(g.GraphNumber, b.GraphNumber)))
                continue;
            disk.MappingRules.Add(CloneMappingRule(b));
            addedRules++;
        }

        return new MergeResult(addedRules, addedGraphs);
    }

    private static bool IsBundledGraphSupersededByRename(
        VerificationConfig disk,
        VerificationConfig bundled,
        string bundledGraphNumber) =>
        bundled.MappingRules.Any(r =>
            SameGraphNumber(r.GraphNumber, bundledGraphNumber) &&
            disk.MappingRules.Any(d =>
                SameMappingKey(d, r) &&
                !bundled.Graphs.Any(bg => SameGraphNumber(bg.GraphNumber, d.GraphNumber))));

    private static bool HasEquivalentMapping(IReadOnlyList<MappingRule> rules, MappingRule candidate) =>
        rules.Any(r => SameMappingKey(r, candidate) && SameGraphNumber(r.GraphNumber, candidate.GraphNumber));

    private static bool SameMappingKey(MappingRule a, MappingRule b) =>
        a.UseProcessSpecs == b.UseProcessSpecs &&
        a.UseMaterial == b.UseMaterial &&
        a.UseSurfaceType == b.UseSurfaceType &&
        a.UseMillingType == b.UseMillingType &&
        a.UseToolType == b.UseToolType &&
        a.UseStrategyType == b.UseStrategyType &&
        string.Equals(Norm(a.ProcessSpecs), Norm(b.ProcessSpecs), StringComparison.Ordinal) &&
        string.Equals(Norm(a.Material), Norm(b.Material), StringComparison.Ordinal) &&
        string.Equals(Norm(a.SurfaceType), Norm(b.SurfaceType), StringComparison.Ordinal) &&
        string.Equals(Norm(a.MillingType), Norm(b.MillingType), StringComparison.Ordinal) &&
        string.Equals(Norm(a.ToolType), Norm(b.ToolType), StringComparison.Ordinal) &&
        string.Equals(Norm(a.StrategyType), Norm(b.StrategyType), StringComparison.Ordinal);

    private static bool SameGraphNumber(string a, string b) =>
        string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);

    private static string Norm(string s) => s.Trim().ToLowerInvariant();

    private static MappingRule CloneMappingRule(MappingRule r) => new()
    {
        ProcessSpecs = r.ProcessSpecs,
        Material = r.Material,
        SurfaceType = r.SurfaceType,
        MillingType = r.MillingType,
        ToolType = r.ToolType,
        StrategyType = r.StrategyType,
        GraphNumber = r.GraphNumber,
        UseProcessSpecs = r.UseProcessSpecs,
        UseMaterial = r.UseMaterial,
        UseSurfaceType = r.UseSurfaceType,
        UseMillingType = r.UseMillingType,
        UseToolType = r.UseToolType,
        UseStrategyType = r.UseStrategyType
    };

    private static ConstraintGraph CloneGraph(ConstraintGraph g) => new()
    {
        GraphNumber = g.GraphNumber,
        CuttingPolygon = g.CuttingPolygon.Select(p => new Point2D(p.X, p.Y)).ToList(),
        EngagementPolygon = g.EngagementPolygon.Select(p => new Point2D(p.X, p.Y)).ToList(),
        EngagementMode = g.EngagementMode,
        AeVsDiameterInequalities = g.AeVsDiameterInequalities.Select(i => new DiameterInequality { Expression = i.Expression }).ToList(),
        ApVsDiameterInequalities = g.ApVsDiameterInequalities.Select(i => new DiameterInequality { Expression = i.Expression }).ToList(),
        AeVsDiameterRange = g.AeVsDiameterRange is null ? null : new DiameterRatioRange { MinD = g.AeVsDiameterRange.MinD, MaxD = g.AeVsDiameterRange.MaxD },
        ApVsDiameterRange = g.ApVsDiameterRange is null ? null : new DiameterRatioRange { MinD = g.ApVsDiameterRange.MinD, MaxD = g.ApVsDiameterRange.MaxD },
        DiameterPlotMaxMm = g.DiameterPlotMaxMm,
        EngagementAeVsDiameterPolygon = g.EngagementAeVsDiameterPolygon.Select(p => new Point2D(p.X, p.Y)).ToList(),
        EngagementApVsDiameterPolygon = g.EngagementApVsDiameterPolygon.Select(p => new Point2D(p.X, p.Y)).ToList()
    };
}
