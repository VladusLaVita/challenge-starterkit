using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ConsoleApp;

public static class PolynomialSolver
{
    public static string Solve(string question)
    {
        if (string.IsNullOrWhiteSpace(question)) return "no roots";

        // Базовая очистка
        string cleaned = question.Replace(" ", "").Replace("(", "").Replace(")", "");
        if (cleaned.EndsWith("=0")) cleaned = cleaned.Substring(0, cleaned.Length - 2);
        cleaned = cleaned.Replace("+-", "-").Replace("-+", "-").Replace("--", "+");

        double a = 0, b = 0, c = 0;

        // 1. Ищем x^2
        var matchA = Regex.Match(cleaned, @"(?<val>[-+]?\d*\.?\d*)\*?x\^2");
        if (matchA.Success)
        {
            a = ParseCoefficient(matchA.Groups["val"].Value, 1.0);
            cleaned = cleaned.Replace(matchA.Value, "");
        }

        // 2. Ищем x (строго БЕЗ ^2 на конце)
        var matchB = Regex.Match(cleaned, @"(?<val>[-+]?\d*\.?\d*)\*?x(?!\^2)");
        if (matchB.Success)
        {
            b = ParseCoefficient(matchB.Groups["val"].Value, 1.0);
            cleaned = cleaned.Replace(matchB.Value, "");
        }

        // 3. Все, что осталось — это свободный член c
        if (!string.IsNullOrWhiteSpace(cleaned))
        {
            // Если остался чистый плюс, например "+5", TryParse с InvariantCulture его поймет.
            // Но на случай если осталось только "+" или "-", превращаем в "0"
            if (cleaned == "+" || cleaned == "-") cleaned = "0";

            if (double.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedC))
            {
                c = parsedC;
            }
        }

        // Логика решения (ваша, она идеальна)
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
        if (string.IsNullOrEmpty(value) || value == "+") return defaultValue;
        if (value == "-") return -defaultValue;

        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
        {
            return result;
        }
        return defaultValue;
    }
}
