using CuttingParameterVerifier.Models;

namespace CuttingParameterVerifier.Services;

/// <summary>Standard CAM relationships between program feed/speed and cutting-chart axes.</summary>
public static class CuttingParameterDerivations
{
    public static void ApplySpeedRateChange(CuttingDataRow row)
    {
        var n = row.ToolSpeedNRpm;
        if (n is not > 0)
            return;

        if (row.DiameterMm is > 0)
            row.SurfaceSpeedVcMMin = Math.PI * row.DiameterMm.Value * n.Value / 1000.0;

        ApplyFeedPerTooth(row);
    }

    public static void ApplyFeedRateChange(CuttingDataRow row) => ApplyFeedPerTooth(row);

    private static void ApplyFeedPerTooth(CuttingDataRow row)
    {
        if (row.FeedRateVfMmMin is > 0 && row.ToolSpeedNRpm is > 0 && row.NumberOfTeethZ is > 0)
            row.FeedPerToothFzMm = row.FeedRateVfMmMin.Value / (row.NumberOfTeethZ.Value * row.ToolSpeedNRpm.Value);
    }
}
