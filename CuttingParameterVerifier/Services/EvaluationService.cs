using CuttingParameterVerifier.Models;

namespace CuttingParameterVerifier.Services;

public sealed class EvaluationService : IEvaluationService
{
    private readonly IMappingService _mapping;
    private readonly IConstraintService _constraints;

    public EvaluationService(IMappingService mapping, IConstraintService constraints)
    {
        _mapping = mapping;
        _constraints = constraints;
    }

    public IReadOnlyList<ResultRow> Evaluate(IReadOnlyList<CuttingDataRow> rows, VerificationConfig config)
    {
        var results = new List<ResultRow>(rows.Count);
        foreach (var row in rows)
        {
            if (!row.IsValid)
            {
                results.Add(new ResultRow
                {
                    Source = row,
                    MatchedGraphNumbers = null,
                    ParameterStatusesPerGraph = null,
                    EngagementStatusesPerGraph = null,
                    ParameterStatus = PassFailNa.Na,
                    EngagementStatus = PassFailNa.Na
                });
                continue;
            }

            var graphIds = _mapping.ResolveGraphNumbers(row, config);
            if (graphIds.Count == 0)
            {
                results.Add(new ResultRow
                {
                    Source = row,
                    MatchedGraphNumbers = null,
                    ParameterStatusesPerGraph = null,
                    EngagementStatusesPerGraph = null,
                    ParameterStatus = PassFailNa.Na,
                    EngagementStatus = PassFailNa.Na
                });
                continue;
            }

            var matched = graphIds;
            var paramStatuses = new List<PassFailNa>(graphIds.Count);
            var engStatuses = new List<PassFailNa>(graphIds.Count);

            foreach (var gid in graphIds)
            {
                var graph = _constraints.FindGraph(config, gid);
                if (graph is null)
                {
                    paramStatuses.Add(PassFailNa.Na);
                    engStatuses.Add(PassFailNa.Na);
                    continue;
                }

                var cutPoly = PolygonNormalizer.EnsureEvaluablePolygon(graph.CuttingPolygon);
                paramStatuses.Add(ConstraintEval.EvaluateCutting(row, cutPoly));
                engStatuses.Add(ConstraintEval.EvaluateEngagementForGraph(row, graph));
            }

            results.Add(new ResultRow
            {
                Source = row,
                MatchedGraphNumbers = matched,
                ParameterStatusesPerGraph = paramStatuses,
                EngagementStatusesPerGraph = engStatuses,
                ParameterStatus = ConstraintEval.AggregateAcrossGraphs(paramStatuses),
                EngagementStatus = ConstraintEval.AggregateAcrossGraphs(engStatuses)
            });
        }

        return results;
    }
}
