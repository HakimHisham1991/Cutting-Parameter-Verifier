using System.Text.Json;
using System.Text.Json.Serialization;
using ClosedXML.Excel;
using CuttingParameterVerifier.Models;
using CuttingParameterVerifier.Services;

static string? FindVerifierProjectDirectory()
{
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir is not null)
    {
        var nested = Path.Combine(dir.FullName, "CuttingParameterVerifier", "CuttingParameterVerifier.csproj");
        if (File.Exists(nested))
            return Path.GetDirectoryName(nested)!;

        var flat = Path.Combine(dir.FullName, "CuttingParameterVerifier.csproj");
        if (File.Exists(flat))
            return dir.FullName;

        dir = dir.Parent;
    }

    return null;
}

var verifierDir = FindVerifierProjectDirectory()
                  ?? throw new InvalidOperationException(
                      "Run from the repository so CuttingParameterVerifier.csproj can be located.");

var samplesDir = Path.Combine(verifierDir, "wwwroot", "samples");
var dataDir = Path.Combine(verifierDir, "Data");
Directory.CreateDirectory(samplesDir);
Directory.CreateDirectory(dataDir);

var xlsxPath = Path.Combine(samplesDir, "sample.xlsx");
var preamblePath = Path.Combine(samplesDir, "sample-with-preamble.xlsx");
var jsonPath = Path.Combine(dataDir, "sample-constraints.json");

WriteSampleWorkbook(xlsxPath);
WriteSampleWorkbook(preamblePath, headerRow: 4, ws =>
{
    ws.Cell(1, 1).Value = "CAM cutting parameter export";
    ws.Cell(2, 1).Value = "Program: DEMO-001";
    ws.Cell(3, 1).Value = "Exported: 2026-05-29";
});
WriteSampleJson(jsonPath);

var constraintConfig = DefaultVerificationConfigFactory.Create();
var excel = new ExcelService(new StaticConstraintService(constraintConfig));
await VerifyImport(excel, xlsxPath, expectedRows: 4);
await VerifyImport(excel, preamblePath, expectedRows: 4);

Console.WriteLine($"Wrote {xlsxPath}");
Console.WriteLine($"Wrote {preamblePath}");
Console.WriteLine($"Wrote {jsonPath}");

static async Task VerifyImport(ExcelService excel, string path, int expectedRows)
{
    await using var stream = File.OpenRead(path);
    var rows = await excel.ReadAsync(stream);
    if (rows.Count != expectedRows)
        throw new InvalidOperationException($"Expected {expectedRows} rows from {path}, got {rows.Count}.");
}

static void WriteSampleWorkbook(string path, int headerRow = 1, Action<IXLWorksheet>? addPreamble = null)
{
    using var wb = new XLWorkbook();
    var ws = wb.AddWorksheet("CAM");
    addPreamble?.Invoke(ws);

    var headers = new[]
    {
        "No.", "A/C Type", "Part Number", "Material Type", "Tool Ref. Number", "Cutter Description",
        "Cutter Type", "Tool Type (Carbide/HSS/PCD)", "Machining Type (Conventional/HSM)",
        "Finish Type (Finish / Controlled Roughing / Free Roughing)",
        "Tool Diameter (mm)", "Number of Flutes (teeth)", "Feed Rate 100% (mm/min)", "Speed Rate 100% (rpm)",
        "Axial (ap) D.O.C (mm)", "Radial (ae) D.O.C (mm)", "Feed per Tooth [Fz] (mm/tooth)", "Speed Vc (m/min)",
        "Justification", "Ramp Angle (Deg)", "Approach / Plunge Feed (mm/min)", "Operation Name"
    };

    for (var c = 0; c < headers.Length; c++)
        ws.Cell(headerRow, c + 1).Value = headers[c];

    var firstDataRow = headerRow + 1;

    void Row(int r, object[] values)
    {
        for (var c = 0; c < values.Length; c++)
        {
            var cell = ws.Cell(r, c + 1);
            switch (values[c])
            {
                case int i:
                    cell.Value = i;
                    break;
                case double d:
                    cell.Value = d;
                    break;
                case string s:
                    cell.Value = s;
                    break;
                default:
                    cell.Value = values[c]?.ToString() ?? "";
                    break;
            }
        }
    }

    Row(firstDataRow, new object[]
    {
        1, "", "", "Aluminium", "", "EM10", "End Milling", "Carbide", "Conventional", "Finish",
        10, 3, 2400, 12000, 1.5, 2.0, 0.12, 600, "", "", "", "Finish floor"
    });

    Row(firstDataRow + 1, new object[]
    {
        2, "", "", "Aluminium", "", "EM10", "End Milling", "Carbide", "Conventional", "Finish",
        10, 3, 2400, 12000, 1.5, 2.0, 0.35, 600, "", "", "", "Finish wall"
    });

    Row(firstDataRow + 2, new object[]
    {
        3, "", "", "Aluminium", "", "EM12", "End Milling", "Carbide", "Conventional", "Controlled Roughing",
        12, 4, 3600, 9000, 3.0, 4.0, 0.08, 400, "", "", "", "Rough pocket"
    });

    Row(firstDataRow + 3, new object[]
    {
        4, "", "", "Aluminium", "", "EM10", "End Milling", "Carbide", "Conventional", "Free Roughing",
        10, 3, 2400, 12000, 1.5, 2.0, "", "", "", "", "", "Invalid row demo"
    });

    ws.Columns().AdjustToContents();
    wb.SaveAs(path);
}

static void WriteSampleJson(string path)
{
    var cfg = DefaultVerificationConfigFactory.Create();
    var opts = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    File.WriteAllText(path, JsonSerializer.Serialize(cfg, opts));
}

file sealed class StaticConstraintService : IConstraintService
{
    private readonly VerificationConfig _config;

    public StaticConstraintService(VerificationConfig config) => _config = config;

    public event Action? ConfigurationChanged;

    public VerificationConfig Load() => _config;

    public void Save(VerificationConfig config) => throw new NotSupportedException();

    public ConstraintGraph? FindGraph(VerificationConfig config, string graphNumber) =>
        config.Graphs.FirstOrDefault(g =>
            string.Equals(g.GraphNumber, graphNumber, StringComparison.OrdinalIgnoreCase));
}
