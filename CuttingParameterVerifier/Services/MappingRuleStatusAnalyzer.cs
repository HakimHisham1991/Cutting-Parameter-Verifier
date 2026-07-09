using CuttingParameterVerifier.Models;

namespace CuttingParameterVerifier.Services;

public static class MappingRuleStatusAnalyzer
{
    public static IReadOnlyList<MappingRuleStatus> Analyze(
        IReadOnlyList<MappingRule> rules,
        IReadOnlyList<ConstraintGraph> graphs)
    {
        var statuses = new MappingRuleStatus[rules.Count];
        for (var i = 0; i < rules.Count; i++)
            statuses[i] = AnalyzeRule(rules[i], rules, i, graphs);
        return statuses;
    }

    public static MappingRuleStatus AnalyzeRule(
        MappingRule rule,
        IReadOnlyList<MappingRule> rules,
        int index,
        IReadOnlyList<ConstraintGraph> graphs)
    {
        if (!IsMappedToGraph(rule, graphs))
            return MappingRuleStatus.NotUsed;

        for (var j = 0; j < rules.Count; j++)
        {
            if (j == index)
                continue;

            if (MappingRuleMatchLogic.SameMappingKey(rule, rules[j]))
                return MappingRuleStatus.Duplicate;
        }

        return MappingRuleStatus.Ok;
    }

    private static bool IsMappedToGraph(MappingRule rule, IReadOnlyList<ConstraintGraph> graphs)
    {
        var graphNumber = rule.GraphNumber.Trim();
        if (graphNumber.Length == 0)
            return false;

        return graphs.Any(g => MappingRuleMatchLogic.SameGraphNumber(g.GraphNumber, graphNumber));
    }
}
