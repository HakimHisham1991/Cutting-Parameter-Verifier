using System.Text.Json;
using System.Text.Json.Serialization;
using CuttingParameterVerifier.Models;

namespace CuttingParameterVerifier.Services;

public sealed class ConstraintService : IConstraintService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ConstraintService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public event Action? ConfigurationChanged;

    public ConstraintService(IWebHostEnvironment env, ILogger<ConstraintService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public VerificationConfig Load()
    {
        var path = GetConfigPath();
        try
        {
            if (!File.Exists(path))
            {
                var created = DefaultVerificationConfigFactory.Create();
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, JsonSerializer.Serialize(created, JsonOptions));
                _logger.LogInformation("Created default constraints at {Path}", path);
                return created;
            }

            var json = File.ReadAllText(path);
            var cfg = JsonSerializer.Deserialize<VerificationConfig>(json, JsonOptions);
            return cfg ?? DefaultVerificationConfigFactory.Create();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load constraints; using defaults.");
            return DefaultVerificationConfigFactory.Create();
        }
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
}
