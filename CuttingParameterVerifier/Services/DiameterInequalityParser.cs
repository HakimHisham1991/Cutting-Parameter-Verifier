using System.Globalization;
using System.Text.RegularExpressions;

namespace CuttingParameterVerifier.Services;

/// <summary>Parses ae/ap vs Ø inequality expressions such as <c>ae &lt;= 1*D</c>.</summary>
public static partial class DiameterInequalityParser
{
    private const double Epsilon = 1e-9;

    public sealed class ParsedConstraint
    {
        public required string Variable { get; init; }
        public required string Operator { get; init; }
        public required string RawExpression { get; init; }
        public double Constant { get; init; }
        public double DCoefficient { get; init; }

        public double BoundAt(double diameterMm) => Constant + DCoefficient * diameterMm;

        public bool Satisfies(double diameterMm, double valueMm)
        {
            var bound = BoundAt(diameterMm);
            return Operator switch
            {
                ">=" => valueMm >= bound - Epsilon,
                "<=" => valueMm <= bound + Epsilon,
                ">" => valueMm > bound + Epsilon,
                "<" => valueMm < bound - Epsilon,
                _ => false
            };
        }

        /// <summary>Half-plane clip: keep points where the inequality holds.</summary>
        public (double A, double B, double C) ToHalfPlaneCoefficients()
        {
            // y >= k*x + c  =>  -k*x + y - c >= 0
            // y <= k*x + c  =>   k*x - y + c >= 0
            return Operator switch
            {
                ">=" or ">" => (-DCoefficient, 1, -Constant),
                "<=" or "<" => (DCoefficient, -1, Constant),
                _ => (0, 0, -1)
            };
        }
    }

    public static bool TryParse(string expression, string expectedVariable, out ParsedConstraint parsed, out string? error)
    {
        parsed = null!;
        error = null;

        if (string.IsNullOrWhiteSpace(expression))
        {
            error = "Expression is empty.";
            return false;
        }

        var normalized = Normalize(expression);
        var match = InequalityRegex().Match(normalized);
        if (!match.Success)
        {
            error = $"Could not parse \"{expression.Trim()}\". Use forms like {expectedVariable} >= 0 or {expectedVariable} <= 1*D.";
            return false;
        }

        var variable = match.Groups["var"].Value.ToLowerInvariant();
        if (!string.Equals(variable, expectedVariable, StringComparison.OrdinalIgnoreCase))
        {
            error = $"Expected variable \"{expectedVariable}\", got \"{variable}\".";
            return false;
        }

        var op = match.Groups["op"].Value;
        var rhs = match.Groups["rhs"].Value;
        if (!TryParseRhs(rhs, out var constant, out var dCoeff, out var rhsError))
        {
            error = rhsError;
            return false;
        }

        parsed = new ParsedConstraint
        {
            Variable = variable,
            Operator = op,
            RawExpression = expression.Trim(),
            Constant = constant,
            DCoefficient = dCoeff
        };
        return true;
    }

    public static bool TryParseAll(
        IEnumerable<string> expressions,
        string expectedVariable,
        out List<ParsedConstraint> parsed,
        out List<string> errors)
    {
        parsed = new List<ParsedConstraint>();
        errors = new List<string>();
        foreach (var expr in expressions)
        {
            if (string.IsNullOrWhiteSpace(expr)) continue;
            if (!TryParse(expr, expectedVariable, out var p, out var err))
            {
                errors.Add(err ?? $"Invalid expression: {expr}");
                continue;
            }
            parsed.Add(p);
        }

        if (parsed.Count == 0 && !expressions.Any(e => !string.IsNullOrWhiteSpace(e)))
            errors.Add($"At least one inequality is required for {expectedVariable} vs Ø.");

        return errors.Count == 0 && parsed.Count > 0;
    }

    private static string Normalize(string s)
    {
        s = s.Trim();
        s = s.Replace('Ø', 'D').Replace('ø', 'D');
        s = s.Replace(" ", "").Replace("\t", "");
        s = s.Replace("≤", "<=").Replace("≥", ">=");
        return s;
    }

    private static bool TryParseRhs(string rhs, out double constant, out double dCoeff, out string? error)
    {
        constant = 0;
        dCoeff = 0;
        error = null;
        rhs = rhs.Trim();

        if (string.Equals(rhs, "D", StringComparison.OrdinalIgnoreCase))
        {
            dCoeff = 1;
            return true;
        }

        if (double.TryParse(rhs, NumberStyles.Float, CultureInfo.InvariantCulture, out constant))
            return true;

        var mulD = Regex.Match(rhs, @"^(?<c>[-+]?(?:\d+\.?\d*|\.\d+))\*(?<d>[dD])$", RegexOptions.IgnoreCase);
        if (mulD.Success &&
            double.TryParse(mulD.Groups["c"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var c1))
        {
            constant = 0;
            dCoeff = c1;
            return true;
        }

        var dMul = Regex.Match(rhs, @"^(?<d>[dD])\*(?<c>[-+]?(?:\d+\.?\d*|\.\d+))$", RegexOptions.IgnoreCase);
        if (dMul.Success &&
            double.TryParse(dMul.Groups["c"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var c2))
        {
            constant = 0;
            dCoeff = c2;
            return true;
        }

        error = $"Could not parse right-hand side \"{rhs}\". Use a number (e.g. 0.2) or D multiple (e.g. 0.5*D).";
        return false;
    }

    [GeneratedRegex(@"^(?<var>ae|ap)(?<op>>=|<=|>|<)(?<rhs>.+)$", RegexOptions.IgnoreCase)]
    private static partial Regex InequalityRegex();
}
