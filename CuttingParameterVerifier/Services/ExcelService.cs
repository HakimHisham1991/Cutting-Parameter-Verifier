using System.Globalization;
using ClosedXML.Excel;
using CuttingParameterVerifier.Models;

namespace CuttingParameterVerifier.Services;

public sealed class ExcelService : IExcelService
{
    public async Task<IReadOnlyList<CuttingDataRow>> ReadAsync(Stream xlsxStream, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        using var workbook = new XLWorkbook(xlsxStream);
        var ws = workbook.Worksheets.FirstOrDefault();
        if (ws is null) return Array.Empty<CuttingDataRow>();

        var headerRow = ws.FirstRowUsed();
        if (headerRow is null) return Array.Empty<CuttingDataRow>();

        var colMap = BuildColumnMap(headerRow);
        var rows = new List<CuttingDataRow>();
        foreach (var row in ws.RowsUsed().Skip(1))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var entity = MapRow(row, colMap);
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
            "No.", "A/C Type", "Part Number", "Material Type", "Tool Ref. Number",
            "Cutter Description", "Cutter Type", "Tool Type (Carbide/HSS/PCD)",
            "Finish Type (Finish / Controlled Roughing / Free Roughing)",
            "Tool Diameter (mm)", "Number of Flutes (teeth)",
            "n (RPM)", "Vf (mm/min)", "Vc (m/min)",
            "Fz (mm)", "ae (mm)", "ap (mm)",
            "Strategy Type", "Operation Name",
            "Figure No.", "Parameter In Spec", "Engagement In Spec", "Remarks"
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
            ws.Cell(r, 3).Value = s.PartNumber;
            ws.Cell(r, 4).Value = s.Material;
            ws.Cell(r, 5).Value = s.ToolRefNumber;
            ws.Cell(r, 6).Value = s.ToolName;
            ws.Cell(r, 7).Value = s.MillingType;
            ws.Cell(r, 8).Value = s.ToolType;
            ws.Cell(r, 9).Value = s.SurfaceType;
            WriteDouble(ws.Cell(r, 10), s.DiameterMm);
            WriteInt(ws.Cell(r, 11), s.NumberOfTeethZ);
            WriteDouble(ws.Cell(r, 12), s.ToolSpeedNRpm);
            WriteDouble(ws.Cell(r, 13), s.FeedRateVfMmMin);
            WriteDouble(ws.Cell(r, 14), s.SurfaceSpeedVcMMin);
            WriteDouble(ws.Cell(r, 15), s.FeedPerToothFzMm);
            WriteDouble(ws.Cell(r, 16), s.RadialDocAeMm);
            WriteDouble(ws.Cell(r, 17), s.AxialDocApMm);
            ws.Cell(r, 18).Value = s.StrategyType;
            ws.Cell(r, 19).Value = s.OperationName;
            ws.Cell(r, 20).Value = res.FigureNumbersDisplay ?? "N/A";
            ws.Cell(r, 21).Value = PassFailListToExcel(res.ParameterStatusesPerGraph, res.ParameterStatus);
            ws.Cell(r, 22).Value = PassFailListToExcel(res.EngagementStatusesPerGraph, res.EngagementStatus);
            ws.Cell(r, 23).Value = s.Remarks;
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

    private static CuttingDataRow MapRow(IXLRow row, Dictionary<string, int> colMap)
    {
        var e = new CuttingDataRow();

        e.No = ReadInt(row, colMap, "no.", "no", "#");
        // Prefer distinctive substrings first so substring matching does not latch onto an unrelated column.
        e.AcType = ReadString(row, colMap, "a/c type");
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

        e.StrategyType = ReadString(row, colMap, "strategy type");

        // New template lists Strategy Type before Operation Name; name is unchanged.
        e.OperationName = ReadString(row, colMap, "operation name");

        Validate(e);
        return e;
    }

    private static void Validate(CuttingDataRow e)
    {
        void Req(string name, bool ok)
        {
            if (!ok) e.ValidationErrors.Add($"{name} is missing or invalid.");
        }

        Req("Vc (surface speed)", e.SurfaceSpeedVcMMin is > 0);
        Req("Fz (feed per tooth)", e.FeedPerToothFzMm is > 0);
        Req("ae (radial DOC)", e.RadialDocAeMm is > 0);
        Req("ap (axial DOC)", e.AxialDocApMm is > 0);
        Req("Material / Material Type", !string.IsNullOrWhiteSpace(e.Material));
        Req("Finish Type / Surface Type", !string.IsNullOrWhiteSpace(e.SurfaceType));
        Req("Cutter Type / Milling Type", !string.IsNullOrWhiteSpace(e.MillingType));
        Req("Tool Type (Carbide/HSS/PCD)", !string.IsNullOrWhiteSpace(e.ToolType));
        Req("Strategy Type", !string.IsNullOrWhiteSpace(e.StrategyType));

        e.IsValid = e.ValidationErrors.Count == 0;
        e.Remarks = e.IsValid ? "" : string.Join("; ", e.ValidationErrors);
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
