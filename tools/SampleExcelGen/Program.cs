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
        "No.", "Operation Name", "Tool Name", "Diameter, Ø (mm)", "Number of teeth/flutes (Z)",
        "Tool Speed, n (RPM)", "Feed Rate, Vf (mm/min)", "Surface Speed, Vc (m/min)", "Feed per tooth, Fz (mm)",
        "Radial D.O.C, ae (mm)", "Axial D.O.C, ap (mm)", "Material", "Surface Type", "Milling Type", "Tool Type",
        "Strategy Type"
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
        1, "Finish floor", "EM10", 10, 3, 12000, 2400, 600, 0.12, 2.0, 1.5, "Aluminum", "Finishing", "End milling",
        "Carbide", "Conventional"
    });

    Row(3, new object[]
    {
        2, "Finish wall", "EM10", 10, 3, 12000, 2400, 600, 0.35, 2.0, 1.5, "Aluminum", "Finishing", "End milling",
        "Carbide", "Conventional"
    });

    Row(4, new object[]
    {
        3, "Rough pocket", "EM12", 12, 4, 9000, 3600, 400, 0.08, 4.0, 3.0, "Steel", "Roughing", "End milling",
        "Carbide", "Conventional"
    });

    Row(5, new object[]
    {
        4, "Invalid row demo", "EM10", 10, 3, 12000, 2400, "", "", 2.0, 1.5, "", "Finishing", "End milling",
        "Carbide", "Conventional"
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
