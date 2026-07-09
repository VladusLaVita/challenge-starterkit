using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ConsoleApp;

public static class StatisticsSolver
{
    public static string Solve(string question)
    {
        if (string.IsNullOrWhiteSpace(question))
            throw new ArgumentException("Пустое условие задачи statistics", nameof(question));

        var parts = question.Split('|');
        if (parts.Length != 2)
            throw new ArgumentException($"Неверный формат вопроса: ожидается 'функция|числа', получено '{question}'");

        var funcName = parts[0].Trim().ToLowerInvariant();
        var numbersStr = parts[1].Trim();
        double[] numbers;
        if (string.IsNullOrWhiteSpace(numbersStr))
        {
            numbers = Array.Empty<double>();
        }
        else
        {
            numbers = numbersStr.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(s => double.Parse(s, CultureInfo.InvariantCulture))
                                .ToArray();
        }

        var result = ApplyFunction(funcName, numbers);
        return FormatResult(result);
    }

    private static object ApplyFunction(string funcName, double[] numbers)
    {
        switch (funcName)
        {
            case "min": return numbers.Length == 0 ? 0.0 : numbers.Min();
            case "max": return numbers.Length == 0 ? 0.0 : numbers.Max();
            case "sum": return numbers.Sum();
            case "mean":
            case "avg":
            case "average":
                return numbers.Length == 0 ? 0.0 : numbers.Average();
            case "median":
                if (numbers.Length == 0) return 0.0;
                var sorted = numbers.OrderBy(x => x).ToArray();
                var mid = sorted.Length / 2;
                return sorted.Length % 2 == 0 ? (sorted[mid - 1] + sorted[mid]) / 2.0 : sorted[mid];
            case "mode":
                if (numbers.Length == 0) return Array.Empty<double>();
                var groups = numbers.GroupBy(x => x).OrderByDescending(g => g.Count()).ThenBy(g => g.Key).ToList();
                var maxCount = groups.First().Count();
                return groups.Where(g => g.Count() == maxCount).Select(g => g.Key).ToArray();
            case "var":
            case "variance":
                if (numbers.Length == 0) return 0.0;
                var mean = numbers.Average();
                return numbers.Sum(x => (x - mean) * (x - mean)) / numbers.Length;
            case "std":
            case "stdev":
            case "sd":
                if (numbers.Length == 0) return 0.0;
                var meanStd = numbers.Average();
                return Math.Sqrt(numbers.Sum(x => (x - meanStd) * (x - meanStd)) / numbers.Length);
            case "count":
            case "len":
            case "length":
                return (double)numbers.Length;
            case "prod":
            case "product":
                return numbers.Aggregate(1.0, (acc, x) => acc * x);
            case "range":
                if (numbers.Length == 0) return 0.0;
                return numbers.Max() - numbers.Min();
            case "gmean":
                if (numbers.Length == 0) return 0.0;
                return Math.Exp(numbers.Average(x => Math.Log(x)));
            case "hmean":
                if (numbers.Length == 0) return 0.0;
                return numbers.Length / numbers.Sum(x => 1.0 / x);
            case "rms":
                if (numbers.Length == 0) return 0.0;
                return Math.Sqrt(numbers.Average(x => x * x));
        }

        switch (funcName)
        {
            case "sort": return numbers.OrderBy(x => x).ToArray();
            case "rsort":
            case "desc": return numbers.OrderByDescending(x => x).ToArray();
            case "reverse": return numbers.Reverse().ToArray();
            case "abs": return numbers.Select(Math.Abs).ToArray();
            case "sin": return numbers.Select(Math.Sin).ToArray();
            case "cos": return numbers.Select(Math.Cos).ToArray();
            case "tan": return numbers.Select(Math.Tan).ToArray();
            case "asin": return numbers.Select(Math.Asin).ToArray();
            case "acos": return numbers.Select(Math.Acos).ToArray();
            case "atan": return numbers.Select(Math.Atan).ToArray();
            case "sqrt": return numbers.Select(Math.Sqrt).ToArray();
            case "exp": return numbers.Select(Math.Exp).ToArray();
            case "log10": return numbers.Select(Math.Log10).ToArray();
            case "ceil":
            case "ceiling": return numbers.Select(Math.Ceiling).ToArray();
            case "floor": return numbers.Select(Math.Floor).ToArray();
            case "round": return numbers.Select(x => Math.Round(x, MidpointRounding.AwayFromZero)).ToArray();
            case "sign": return numbers.Select(x => (double)Math.Sign(x)).ToArray();
            case "square": return numbers.Select(x => x * x).ToArray();
            case "cube": return numbers.Select(x => x * x * x).ToArray();
            case "factorial":
                return numbers.Select(x =>
                {
                    var n = (long)Math.Round(x);
                    if (n < 0) return double.NaN;
                    if (n > 20) return double.PositiveInfinity;
                    long res = 1;
                    for (long i = 2; i <= n; i++) res *= i;
                    return (double)res;
                }).ToArray();
        }

        throw new NotSupportedException($"Функция '{funcName}' не реализована.");
    }

    private static string FormatResult(object result)
    {
        if (result is double single)
        {
            return FormatNumber(single);
        }
        else if (result is double[] array)
        {
            if (array.Length == 0) return string.Empty;
            return string.Join(" ", array.Select(FormatNumber));
        }
        return result?.ToString() ?? string.Empty;
    }

    private static string FormatNumber(double d)
    {
        if (double.IsNaN(d)) return "NaN";
        if (double.IsInfinity(d)) return d.ToString(CultureInfo.InvariantCulture);
        if (d == Math.Floor(d) && Math.Abs(d) < 9e15)
        {
            return ((long)d).ToString(CultureInfo.InvariantCulture);
        }

        return d.ToString("G", CultureInfo.InvariantCulture);
    }
}