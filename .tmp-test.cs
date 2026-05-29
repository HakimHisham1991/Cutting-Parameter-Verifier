using CuttingParameterVerifier.Services;
using ClosedXML.Excel;
var path = @"CuttingParameterVerifier/wwwroot/samples/sample.xlsx";
using var fs = File.OpenRead(path);
var svc = new ExcelService();
var rows = await svc.ReadAsync(fs);
Console.WriteLine($"Rows: {rows.Count}");
foreach (var r in rows.Take(2))
    Console.WriteLine($"  ToolType={r.ToolType}, StrategyType={r.StrategyType}, SurfaceType={r.SurfaceType}, Op={r.OperationName}, Valid={r.IsValid}");
