using CuttingParameterVerifier.Models;

namespace CuttingParameterVerifier.Services;

/// <summary>Default mapping rules and constraint graphs (aluminium library).</summary>
public static class DefaultVerificationConfigFactory
{
    public static VerificationConfig Create()
    {
        var cfg = new VerificationConfig();
        cfg.MappingRules.AddRange(CreateDefaultMappings());
        cfg.Graphs.AddRange(CreateDefaultGraphs());
        foreach (var g in cfg.Graphs)
            NormalizeGraph(g);
        return cfg;
    }

    private static void NormalizeGraph(ConstraintGraph g)
    {
        g.CuttingPolygon = PolygonNormalizer.EnsureEvaluablePolygon(g.CuttingPolygon);
        g.EngagementPolygon = PolygonNormalizer.EnsureEvaluablePolygon(g.EngagementPolygon);
    }

    private static IEnumerable<MappingRule> CreateDefaultMappings()
    {
        yield return Rule("Aluminium", "Roughing", "End milling", "Carbide", "Conventional", "3.2.2.4.1.1");
        yield return Rule("Aluminium", "Finishing", "End milling", "Carbide", "Conventional", "3.2.2.4.1.2");
        yield return Rule("Aluminium", "Roughing", "Face milling", "Carbide", "Conventional", "3.2.2.4.1.3");
        yield return Rule("Aluminium", "Finishing", "Face milling", "Carbide", "Conventional", "3.2.2.4.1.4");
        yield return Rule("Aluminium", "Roughing", "End milling", "HSS", "Conventional", "3.2.2.4.2.1");
        yield return Rule("Aluminium", "Finishing", "End milling", "HSS", "Conventional", "3.2.2.4.2.1");
        yield return Rule("Aluminium", "Roughing", "Face milling", "HSS", "Conventional", "3.2.2.4.2.2");
        yield return Rule("Aluminium", "Finishing", "Face milling", "HSS", "Conventional", "3.2.2.4.2.2");
        yield return Rule("Aluminium", "Roughing", "End milling", "Carbide", "HSM", "3.2.2.4.3.1");
        yield return Rule("Aluminium", "Finishing", "End milling", "Carbide", "HSM", "3.2.2.4.3.1");
        yield return Rule("Aluminium", "Roughing", "Face milling", "Carbide", "HSM", "3.2.2.4.3.3");
        yield return Rule("Aluminium", "Finishing", "Face milling", "Carbide", "HSM", "3.2.2.4.3.4");
        yield return Rule("Aluminium", "Roughing", "End milling", "PCD", "HSM", "3.2.2.4.4.1");
        yield return Rule("Aluminium", "Finishing", "End milling", "PCD", "HSM", "3.2.2.4.4.2");
        yield return Rule("Aluminium", "Roughing", "Face milling", "PCD", "HSM", "3.2.2.4.4.3");
        yield return Rule("Aluminium", "Finishing", "Face milling", "PCD", "HSM", "3.2.2.4.4.4");
    }

    private static MappingRule Rule(string m, string s, string mi, string t, string st, string graph) => new()
    {
        Material = m,
        SurfaceType = s,
        MillingType = mi,
        ToolType = t,
        StrategyType = st,
        GraphNumber = graph
    };

    private static List<Point2D> P(params (double X, double Y)[] pts) =>
        pts.Select(p => new Point2D(p.X, p.Y)).ToList();

    private static List<ConstraintGraph> CreateDefaultGraphs()
    {
        return new List<ConstraintGraph>
        {
            new()
            {
                GraphNumber = "3.2.2.4.1.1",
                CuttingPolygon = P(
                    (57, 0.05), (57, 0.25), (113, 0.25), (113, 0.35), (1800, 0.35), (1800, 0.021), (704, 0.021)),
                EngagementPolygon = P((12, 6), (20, 6))
            },
            new()
            {
                GraphNumber = "3.2.2.4.1.2",
                CuttingPolygon = P((57, 0.05), (57, 0.25), (1800, 0.25), (1800, 0.021), (704, 0.021)),
                EngagementPolygon = P((0.1, 0.1), (0.1, 20), (7, 20), (100, 0.5), (100, 0.1))
            },
            new()
            {
                GraphNumber = "3.2.2.4.1.3",
                CuttingPolygon = P((151, 0.07), (151, 0.35), (1800, 0.35), (1800, 0.044), (377, 0.044)),
                EngagementPolygon = P((12, 50), (14.5, 25), (25, 25), (25, 0))
            },
            new()
            {
                GraphNumber = "3.2.2.4.1.4",
                CuttingPolygon = P((151, 0.07), (151, 0.25), (1800, 0.25), (1800, 0.044), (377, 0.044)),
                EngagementPolygon = P((4, 50), (16.5, 6), (16.5, 0.1), (0.1, 0.1))
            },
            new()
            {
                GraphNumber = "3.2.2.4.2.1",
                CuttingPolygon = P((151, 0.07), (151, 0.15), (600, 0.15), (600, 0.07)),
                EngagementPolygon = P((50, 50))
            },
            new()
            {
                GraphNumber = "3.2.2.4.2.2",
                CuttingPolygon = P((151, 0.07), (151, 0.15), (600, 0.15), (600, 0.07)),
                EngagementPolygon = P((50, 50))
            },
            new()
            {
                GraphNumber = "3.2.2.4.3.1",
                CuttingPolygon = P((1800, 0.06), (1800, 0.35), (3300, 0.35), (3770, 0.22), (3770, 0.06)),
                EngagementPolygon = P((20, 25), (25, 25), (35, 12.5), (35, 9), (100, 0.1))
            },
            new()
            {
                GraphNumber = "3.2.2.4.3.3",
                CuttingPolygon = P((1800, 0.07), (1800, 0.35), (5429, 0.35), (5429, 0.07)),
                EngagementPolygon = P((20, 28.33), (25, 25))
            },
            new()
            {
                GraphNumber = "3.2.2.4.3.4",
                CuttingPolygon = P((1800, 0.07), (1800, 0.25), (5429, 0.25), (5429, 0.07), (2670, 0.03)),
                EngagementPolygon = P((3, 63), (4, 63), (16.5, 6))
            },
            new()
            {
                GraphNumber = "3.2.2.4.4.1",
                CuttingPolygon = P((1759, 0.25), (2199, 0.25), (3600, 0.15), (3600, 0.07), (1759, 0.07)),
                EngagementPolygon = P((8, 12), (12, 12), (12, 0.1))
            },
            new()
            {
                GraphNumber = "3.2.2.4.4.2",
                CuttingPolygon = P((1759, 0.25), (2199, 0.25), (3600, 0.15), (3600, 0.07), (1759, 0.07)),
                EngagementPolygon = P((0.1, 0.1), (0.1, 12), (12, 12), (8, 8), (8, 0.1))
            },
            new()
            {
                GraphNumber = "3.2.2.4.4.3",
                CuttingPolygon = P((1759, 0.25), (2199, 0.25), (3600, 0.15), (3600, 0.07), (1759, 0.07)),
                EngagementPolygon = P((50, 50))
            },
            new()
            {
                GraphNumber = "3.2.2.4.4.4",
                CuttingPolygon = P((1759, 0.07), (1759, 0.25), (2199, 0.25), (3600, 0.15), (3600, 0.07)),
                EngagementPolygon = P((50, 50))
            }
        };
    }
}
