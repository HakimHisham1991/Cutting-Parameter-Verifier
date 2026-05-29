using CuttingParameterVerifier.Services;
using CuttingParameterVerifier.Models;
using System.Text.Json;
var path = @"c:\Users\Public\Documents\Cutting-Parameter-Verifier\EXCEL SAMPLE\TEST.xlsx";
var excel = new ExcelService();
await using var fs = File.OpenRead(path);
var rows = await excel.ReadAsync(fs);
var cfg = JsonSerializer.Deserialize<VerificationConfig>(await File.ReadAllTextAsync(@"CuttingParameterVerifier/Data/constraints.json"), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
var map = new MappingService();
Console.WriteLine("Distinct Material values: " + string.Join(", ", rows.Select(r => r.Material).Distinct()));
Console.WriteLine($"Valid rows: {rows.Count(r => r.IsValid)} / {rows.Count}");
Console.WriteLine($"Rows with graph match: {rows.Count(r => r.IsValid && map.ResolveGraphNumbers(r, cfg).Count > 0)}");
foreach (var grp in rows.Where(r=>r.IsValid).GroupBy(r => $"{r.Material}|{r.SurfaceType}|{r.MillingType}|{r.ToolType}|{r.StrategyType}")) {
  var g = map.ResolveGraphNumbers(grp.First(), cfg);
  Console.WriteLine($"{grp.Key()} -> {g.Count} graphs ({grp.Count()} rows)");
}
