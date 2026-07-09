using System.Globalization;
using ClosedXML.Excel;
using CuttingParameterVerifier.Models;

namespace CuttingParameterVerifier.Services;

public sealed class ExcelService : IExcelService
{
    private readonly IConstraintService _constraints;

    public ExcelService(IConstraintService constraints)
    {
        _constraints = constraints;
    }

    /// <summary>Distinctive header groups used to locate the column header row (supports title rows above the table).</summary>
    private static readonly string[][] HeaderSignatures =
    [
        ["material type", "material"],
        ["operation name"],
        ["machining type", "strategy type"],
        ["cutter type", "milling type"],
        ["tool type (carbide", "carbide/hss/pcd", "tool type"],
        ["finish type", "surface type"],
        ["part number"],
        ["no.", "no", "#"],
    ];

    private const int MinHeaderSignatureMatches = 4;
    private const int MaxHeaderScanRows = 50;

    public async Task<IReadOnlyList<CuttingDataRow>> ReadAsync(Stream xlsxStream, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        using var workbook = new XLWorkbook(xlsxStream);
        var ws = workbook.Worksheets.FirstOrDefault();
        if (ws is null) return Array.Empty<CuttingDataRow>();

        var headerRow = FindHeaderRow(ws);
        if (headerRow is null) return Array.Empty<CuttingDataRow>();

        var colMap = BuildColumnMap(headerRow);
        var headerRowNumber = headerRow.RowNumber();
        var knownMapping = CuttingDataRowValidator.GetKnownMappingValues(_constraints.Load());
        var rows = new List<CuttingDataRow>();
        foreach (var row in ws.RowsUsed().Where(r => r.RowNumber() > headerRowNumber))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (IsBlankDataRow(row, colMap))
                continue;

            var entity = MapRow(row, colMap, knownMapping);
            rows.Add(entity);
        }

        return rows;
    }

    public async Task WriteResultsAsync(Stream outputStream, IReadOnlyList<ResultRow> results, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Results");
        var headers = new[]
        {
            "No.", "A/C Type", "Process Specs", "Part Number", "Material Type", "Tool Ref. Number",
            "Cutter Description", "Cutter Type", "Tool Type (Carbide/HSS/PCD)",
            "Machining Type (Conventional/HSM)",
            "Finish Type (Finish / Controlled Roughing / Free Roughing)",
            "Tool Diameter (mm)", "Number of Flutes (teeth)",
            "n (RPM)", "Vf (mm/min)", "Vc (m/min)",
            "Fz (mm)", "ae (mm)", "ap (mm)",
            "Operation Name",
            "Figure No.", "Parameter In Spec", "Engagement In Spec", "ae check", "ap check", "Remarks"
        };

        for (var c = 0; c < headers.Length; c++)
            ws.Cell(1, c + 1).Value = headers[c];

        var r = 2;
        foreach (var res in results)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var s = res.Source;
            WriteInt(ws.Cell(r, 1), s.No);
            ws.Cell(r, 2).Value = s.AcType;
            ws.Cell(r, 3).Value = s.ProcessSpecs;
            ws.Cell(r, 4).Value = s.PartNumber;
            ws.Cell(r, 5).Value = s.Material;
            ws.Cell(r, 6).Value = s.ToolRefNumber;
            ws.Cell(r, 7).Value = s.ToolName;
            ws.Cell(r, 8).Value = s.MillingType;
            ws.Cell(r, 9).Value = s.ToolType;
            ws.Cell(r, 10).Value = s.StrategyType;
            ws.Cell(r, 11).Value = s.SurfaceType;
            WriteDouble(ws.Cell(r, 12), s.DiameterMm);
            WriteInt(ws.Cell(r, 13), s.NumberOfTeethZ);
            WriteDouble(ws.Cell(r, 14), s.ToolSpeedNRpm);
            WriteDouble(ws.Cell(r, 15), s.FeedRateVfMmMin);
            WriteDouble(ws.Cell(r, 16), s.SurfaceSpeedVcMMin);
            WriteDouble(ws.Cell(r, 17), s.FeedPerToothFzMm);
            WriteDouble(ws.Cell(r, 18), s.RadialDocAeMm);
            WriteDouble(ws.Cell(r, 19), s.AxialDocApMm);
            ws.Cell(r, 20).Value = s.OperationName;
            ws.Cell(r, 21).Value = res.FigureNumbersDisplay ?? "N/A";
            ws.Cell(r, 22).Value = PassFailListToExcel(res.ParameterStatusesPerGraph, res.ParameterStatus);
            ws.Cell(r, 23).Value = PassFailListToExcel(res.EngagementStatusesPerGraph, res.EngagementStatus);
            ws.Cell(r, 24).Value = PassFailListToExcel(res.AeCheckStatusesPerGraph, res.AeCheckStatus);
            ws.Cell(r, 25).Value = PassFailListToExcel(res.ApCheckStatusesPerGraph, res.ApCheckStatus);
            ws.Cell(r, 26).Value = s.Remarks;
            r++;
        }

        ws.Columns().AdjustToContents();
        wb.SaveAs(outputStream);
    }

    private static void WriteInt(IXLCell cell, int? value)
    {
        if (value.HasValue) cell.Value = value.Value;
        else cell.Clear();
    }

    private static void WriteDouble(IXLCell cell, double? value)
    {
        if (value.HasValue) cell.Value = value.Value;
        else cell.Clear();
    }

    private static string PassFailToString(PassFailNa v) => v switch
    {
        PassFailNa.Pass => "Pass",
        PassFailNa.Fail => "Fail",
        _ => "N/A"
    };

    /// <summary>Comma-separated order matches Figure No. when multiple graphs apply.</summary>
    private static string PassFailListToExcel(IReadOnlyList<PassFailNa>? perGraph, PassFailNa aggregate) =>
        perGraph is { Count: > 0 } ? string.Join(", ", perGraph.Select(PassFailToString)) : PassFailToString(aggregate);

    private static IXLRow? FindHeaderRow(IXLWorksheet ws)
    {
        var firstUsed = ws.FirstRowUsed()?.RowNumber();
        if (firstUsed is null) return null;

        var lastRow = ws.LastRowUsed()?.RowNumber() ?? firstUsed.Value;
        var scanThrough = Math.Min(lastRow, firstUsed.Value + MaxHeaderScanRows - 1);

        IXLRow? bestRow = null;
        var bestScore = 0;

        for (var rowNumber = firstUsed.Value; rowNumber <= scanThrough; rowNumber++)
        {
            var row = ws.Row(rowNumber);
            if (!row.CellsUsed().Any())
                continue;

            var colMap = BuildColumnMap(row);
            var score = ScoreColumnMap(colMap);
            if (score > bestScore)
            {
                bestScore = score;
                bestRow = row;
            }
        }

        if (bestRow is not null && bestScore >= MinHeaderSignatureMatches)
            return bestRow;

        return ws.FirstRowUsed();
    }

    private static int ScoreColumnMap(Dictionary<string, int> map)
    {
        var score = 0;
        foreach (var group in HeaderSignatures)
        {
            if (FindColumn(map, group) is not null)
                score++;
        }

        return score;
    }

    private static bool IsBlankDataRow(IXLRow row, Dictionary<string, int> colMap)
    {
        foreach (var col in colMap.Values)
        {
            var cell = row.Cell(col);
            if (cell.IsEmpty()) continue;
            if (!string.IsNullOrWhiteSpace(cell.GetString())) return false;
        }

        return true;
    }

    private static Dictionary<string, int> BuildColumnMap(IXLRow headerRow)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in headerRow.CellsUsed())
        {
            var key = NormalizeHeader(cell.GetString());
            if (string.IsNullOrEmpty(key)) continue;
            map[key] = cell.Address.ColumnNumber;
        }

        return map;
    }

    private static string NormalizeHeader(string raw) =>
        raw.Trim().ToLowerInvariant().Replace("  ", " ");

    private static CuttingDataRow MapRow(IXLRow row, Dictionary<string, int> colMap, CuttingDataRowValidator.KnownMappingValues knownMapping)
    {
        var e = new CuttingDataRow();

        e.No = ReadInt(row, colMap, "no.", "no", "#");
        // Prefer distinctive substrings first so substring matching does not latch onto an unrelated column.
        e.AcType = ReadString(row, colMap, "a/c type");
        e.ProcessSpecs = ReadString(row, colMap, "process specs");
        e.PartNumber = ReadString(row, colMap, "part number");
        e.ToolRefNumber = ReadString(row, colMap, "tool ref", "tool ref.");

        e.Material = ReadString(row, colMap, "material type", "material");

        // "Cutter Description" (new); "Tool Name" (legacy CAM export).
        e.ToolName = ReadString(row, colMap, "cutter description", "tool name");

        // "Cutter Type" replaces "Milling Type".
        e.MillingType = ReadString(row, colMap, "cutter type", "milling type");

        // e.g. "Tool Type (Carbide/HSS/PCD)" — match the parenthetical variant before bare "tool type".
        e.ToolType = ReadString(
            row,
            colMap,
            "tool type (carbide",
            "carbide/hss/pcd",
            "tool type");

        // "Machining Type (Conventional/HSM)" replaces "Strategy Type" (legacy position at end of row).
        e.StrategyType = ReadString(row, colMap, "machining type", "strategy type");

        // "Finish Type (...)" replaces "Surface Type".
        e.SurfaceType = ReadString(row, colMap, "finish type", "surface type");

        e.DiameterMm = ReadDouble(
            row,
            colMap,
            "tool diameter",
            "diameter",
            "ø (mm)",
            "diameter, ø (mm)");

        e.NumberOfTeethZ = ReadInt(
            row,
            colMap,
            "number of flutes",
            "flutes (teeth)",
            "number of teeth",
            "teeth/flutes",
            "teeth",
            "flutes");

        e.FeedRateVfMmMin = ReadDouble(
            row,
            colMap,
            "feed rate 100%",
            "feed rate",
            "vf (mm/min)",
            "vf");

        e.ToolSpeedNRpm = ReadDouble(
            row,
            colMap,
            "speed rate 100%",
            "tool speed",
            "n (rpm)",
            "rpm");

        e.AxialDocApMm = ReadDouble(row, colMap, "axial (ap)", "axial d.o.c", "axial", "ap (mm)", "ap");
        e.RadialDocAeMm = ReadDouble(row, colMap, "radial (ae)", "radial d.o.c", "radial", "ae (mm)", "ae");

        e.FeedPerToothFzMm = ReadDouble(
            row,
            colMap,
            "feed per tooth",
            "mm/tooth",
            "fz (mm)",
            "fz");

        // "Speed Vc (m/min)" vs legacy "Surface Speed, Vc (m/min)"
        e.SurfaceSpeedVcMMin = ReadDouble(
            row,
            colMap,
            "speed vc",
            "surface speed",
            "vc (m/min)",
            "vc");

        e.Justification = ReadString(row, colMap, "justification");

        e.RampAngleDeg = ReadDouble(row, colMap, "ramp angle");

        // Column title uses "Approach / Plunge" — avoid a bare "approach" substring key (could match unrelated headers).
        e.ApproachPlungeFeedMmMin = ReadDouble(row, colMap, "approach / plunge", "plunge feed");

        e.OperationName = ReadString(row, colMap, "operation name");

        CuttingDataRowValidator.Revalidate(e, knownMapping);
        return e;
    }

    private static int? FindColumn(Dictionary<string, int> map, params string[] keys)
    {
        foreach (var key in keys)
        {
            foreach (var kv in map)
            {
                if (kv.Key.Contains(key, StringComparison.OrdinalIgnoreCase) ||
                    key.Contains(kv.Key, StringComparison.OrdinalIgnoreCase))
                    return kv.Value;
            }
        }

        return null;
    }

    private static string ReadString(IXLRow row, Dictionary<string, int> map, params string[] keys)
    {
        var col = FindColumn(map, keys);
        if (col is null) return "";
        return row.Cell(col.Value).GetString().Trim();
    }

    private static int? ReadInt(IXLRow row, Dictionary<string, int> map, params string[] keys)
    {
        var col = FindColumn(map, keys);
        if (col is null) return null;
        var cell = row.Cell(col.Value);
        if (cell.IsEmpty()) return null;
        if (cell.TryGetValue(out int i)) return i;
        if (int.TryParse(cell.GetString(), NumberStyles.Integer, CultureInfo.CurrentCulture, out i)) return i;
        if (int.TryParse(cell.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out i)) return i;
        return null;
    }

    private static double? ReadDouble(IXLRow row, Dictionary<string, int> map, params string[] keys)
    {
        var col = FindColumn(map, keys);
        if (col is null) return null;
        var cell = row.Cell(col.Value);
        if (cell.IsEmpty()) return null;
        if (cell.TryGetValue(out double d)) return d;
        var s = cell.GetString().Trim();
        if (double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out d)) return d;
        if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out d)) return d;
        return null;
    }
}
