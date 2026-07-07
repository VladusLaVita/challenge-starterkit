using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ConsoleApp;

public static class PolynomialSolver
{
    public static string Solve(string question)
    {
        if (string.IsNullOrWhiteSpace(question)) return "no roots";

        string cleaned = question.Replace(" ", "").Replace("(", "").Replace(")", "");
        if (cleaned.EndsWith("=0")) cleaned = cleaned.Substring(0, cleaned.Length - 2);

        cleaned = cleaned.Replace("+-", "-").Replace("-+", "-").Replace("--", "+");

        double a = 0, b = 0, c = 0;

        var matchA = Regex.Match(cleaned, @"(?<val>[-+]?\d*\.?\d*)\*?x\^2");
        if (matchA.Success)
        {
            a = ParseCoefficient(matchA.Value.Replace("x^2", ""), 1.0);
            cleaned = cleaned.Replace(matchA.Value, "");
        }

        var matchB = Regex.Match(cleaned, @"(?<val>[-+]?\d*\.?\d*)\*?x");
        if (matchB.Success)
        {
            b = ParseCoefficient(matchB.Value.Replace("x", ""), 1.0);
            cleaned = cleaned.Replace(matchB.Value, "");
        }

        if (!string.IsNullOrWhiteSpace(cleaned))
        {
            if (cleaned == "+" || cleaned == "-") cleaned = "0";
            double.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out c);
        }

        if (Math.Abs(a) < 1e-9)
        {
            if (Math.Abs(b) < 1e-9) return "no roots";
            double linearRoot = -c / b;
            return linearRoot.ToString("G17", CultureInfo.InvariantCulture);
        }

        double discriminant = b * b - 4 * a * c;

        if (discriminant < -1e-9)
        {
            return "no roots";
        }

        if (Math.Abs(discriminant) < 1e-9)
        {
            double root = -b / (2 * a);
            return root.ToString("G17", CultureInfo.InvariantCulture);
        }
        else
        {
            double root1 = (-b - Math.Sqrt(discriminant)) / (2 * a);
            double root2 = (-b + Math.Sqrt(discriminant)) / (2 * a);

            double maxRoot = Math.Max(root1, root2);
            return maxRoot.ToString("G17", CultureInfo.InvariantCulture);
        }
    }

    private static double ParseCoefficient(string value, double defaultValue)
    {
        value = value.Replace("*", "");
        if (string.IsNullOrEmpty(value) || value == "+") return defaultValue;
        if (value == "-") return -defaultValue;

        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
        {
            return result;
        }
        return defaultValue;
    }
}
