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
var jsonPath = Path.Combine(dataDir, "sample-constraints.json");

WriteSampleWorkbook(xlsxPath);
WriteSampleJson(jsonPath);

Console.WriteLine($"Wrote {xlsxPath}");
Console.WriteLine($"Wrote {jsonPath}");

static void WriteSampleWorkbook(string path)
{
    using var wb = new XLWorkbook();
    var ws = wb.AddWorksheet("CAM");

    var headers = new[]
    {
        "No.", "A/C Type", "Part Number", "Material Type", "Tool Ref. Number", "Cutter Description",
        "Cutter Type", "Tool Type (Carbide/HSS/PCD)",
        "Finish Type (Finish / Controlled Roughing / Free Roughing)",
        "Tool Diameter (mm)", "Number of Flutes (teeth)", "Feed Rate 100% (mm/min)", "Speed Rate 100% (rpm)",
        "Axial (ap) D.O.C (mm)", "Radial (ae) D.O.C (mm)", "Feed per Tooth [Fz] (mm/tooth)", "Speed Vc (m/min)",
        "Justification", "Ramp Angle (Deg)", "Approach / Plunge Feed (mm/min)", "Strategy Type", "Operation Name"
    };

    for (var c = 0; c < headers.Length; c++)
        ws.Cell(1, c + 1).Value = headers[c];

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

    Row(2, new object[]
    {
        1, "", "", "Aluminum", "", "EM10", "End milling", "Carbide", "Finishing",
        10, 3, 2400, 12000, 1.5, 2.0, 0.12, 600, "", "", "", "Conventional", "Finish floor"
    });

    Row(3, new object[]
    {
        2, "", "", "Aluminum", "", "EM10", "End milling", "Carbide", "Finishing",
        10, 3, 2400, 12000, 1.5, 2.0, 0.35, 600, "", "", "", "Conventional", "Finish wall"
    });

    Row(4, new object[]
    {
        3, "", "", "Steel", "", "EM12", "End milling", "Carbide", "Roughing",
        12, 4, 3600, 9000, 3.0, 4.0, 0.08, 400, "", "", "", "Conventional", "Rough pocket"
    });

    Row(5, new object[]
    {
        4, "", "", "", "", "EM10", "End milling", "Carbide", "Finishing",
        10, 3, 2400, 12000, 1.5, 2.0, "", "", "", "", "", "Conventional", "Invalid row demo"
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
