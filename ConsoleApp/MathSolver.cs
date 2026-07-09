using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using NCalc;

namespace ConsoleApp;

public static class MathSolver
{
    public static string Solve(string question)
    {
        if (string.IsNullOrWhiteSpace(question))
            throw new ArgumentException("Пустое условие задачи math", nameof(question));

        var normalized = Normalize(question);

        try
        {
            var parser = new ExpressionParser(normalized);
            var result = parser.Parse();
            var answer = ToIntegerAnswer(result);

            if (parser.ContainsRoman && result.IsInteger)
            {
                var rounded = (long)Math.Round(result.Value, MidpointRounding.AwayFromZero);
                if (rounded > 0 && rounded <= 3999)
                    return RomanNumerals.ToRoman((int)rounded);
            }

            return answer;
        }
        catch
        {
            var expression = new Expression(normalized, ExpressionOptions.IgnoreCaseAtBuiltInFunctions);
            object rawResult;
            try
            {
                rawResult = expression.Evaluate();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Не удалось вычислить выражение \"{question}\" (нормализовано в \"{normalized}\")", ex);
            }
            return ToIntegerAnswerNCalc(rawResult);
        }
    }

    private static string Normalize(string question)
    {
        var normalized = question.Trim();
        normalized = normalized.Replace("**", "^");
        normalized = Regex.Replace(normalized, @"\s+", "");
        return normalized;
    }

    private static string ToIntegerAnswer(NumberValue value)
    {
        var rounded = (long)Math.Round(value.Value, MidpointRounding.AwayFromZero);
        return rounded.ToString(CultureInfo.InvariantCulture);
    }

    private static string ToIntegerAnswerNCalc(object rawResult)
    {
        if (rawResult == null)
            throw new InvalidOperationException("NCalc вернул null вместо числового результата");
        var value = Convert.ToDouble(rawResult, CultureInfo.InvariantCulture);
        var rounded = (long)Math.Round(value, MidpointRounding.AwayFromZero);
        return rounded.ToString(CultureInfo.InvariantCulture);
    }
}

internal static class RomanNumerals
{
    private static readonly Dictionary<char, int> RomanMap = new Dictionary<char, int>
    {
        {'I', 1}, {'V', 5}, {'X', 10}, {'L', 50},
        {'C', 100}, {'D', 500}, {'M', 1000}
    };

    private static readonly int[] Values = { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
    private static readonly string[] Symbols = { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };

    public static bool TryParse(string s, out int value)
    {
        value = 0;
        if (string.IsNullOrEmpty(s)) return false;

        var upper = s.ToUpperInvariant();
        foreach (var c in upper)
        {
            if (!RomanMap.ContainsKey(c)) return false;
        }

        var prev = 0;
        for (var i = upper.Length - 1; i >= 0; i--)
        {
            var curr = RomanMap[upper[i]];
            if (curr < prev)
                value -= curr;
            else
                value += curr;
            prev = curr;
        }

        return value > 0 && value <= 3999;
    }

    public static string ToRoman(int value)
    {
        if (value <= 0 || value > 3999)
            return value.ToString(CultureInfo.InvariantCulture);

        var result = new StringBuilder();
        for (var i = 0; i < Values.Length; i++)
        {
            while (value >= Values[i])
            {
                result.Append(Symbols[i]);
                value -= Values[i];
            }
        }
        return result.ToString();
    }
}

internal struct NumberValue
{
    public double Value { get; }
    public bool IsInteger { get; }

    public NumberValue(double value, bool isInteger)
    {
        Value = value;
        IsInteger = isInteger;
    }

    public static NumberValue FromLiteral(string literal)
    {
        var value = double.Parse(literal, CultureInfo.InvariantCulture);
        var isInteger = !literal.Contains('.') &&
                        !literal.Contains('e') &&
                        !literal.Contains('E');
        return new NumberValue(value, isInteger);
    }

    public static NumberValue FromRoman(int value) => new NumberValue(value, true);

    public static NumberValue operator +(NumberValue a, NumberValue b)
        => new NumberValue(a.Value + b.Value, a.IsInteger && b.IsInteger);

    public static NumberValue operator -(NumberValue a, NumberValue b)
        => new NumberValue(a.Value - b.Value, a.IsInteger && b.IsInteger);

    public static NumberValue operator *(NumberValue a, NumberValue b)
        => new NumberValue(a.Value * b.Value, a.IsInteger && b.IsInteger);

    public static NumberValue operator /(NumberValue a, NumberValue b)
    {
        if (a.IsInteger && b.IsInteger)
        {
            if (b.Value == 0) throw new DivideByZeroException("Деление на ноль");
            var div = (long)a.Value / (long)b.Value;
            return new NumberValue(div, true);
        }
        return new NumberValue(a.Value / b.Value, false);
    }

    public static NumberValue operator %(NumberValue a, NumberValue b)
    {
        if (a.IsInteger && b.IsInteger)
        {
            if (b.Value == 0) throw new DivideByZeroException("Деление на ноль");
            var mod = (long)a.Value % (long)b.Value;
            return new NumberValue(mod, true);
        }
        return new NumberValue(a.Value % b.Value, false);
    }

    public static NumberValue Pow(NumberValue a, NumberValue b)
        => new NumberValue(Math.Pow(a.Value, b.Value), false);

    public static NumberValue UnaryMinus(NumberValue a)
        => new NumberValue(-a.Value, a.IsInteger);

    public static NumberValue UnaryPlus(NumberValue a) => a;
}

internal class ExpressionParser
{
    private readonly string expression;
    private int position;

    public bool ContainsRoman { get; private set; }

    public ExpressionParser(string expression)
    {
        this.expression = expression;
        position = 0;
    }

    public NumberValue Parse()
    {
        var result = ParseExpression();
        SkipSpaces();
        if (position < expression.Length)
            throw new Exception($"Неожиданный символ на позиции {position}: '{expression[position]}'");
        return result;
    }

    private void SkipSpaces()
    {
        while (position < expression.Length && char.IsWhiteSpace(expression[position]))
            position++;
    }

    private NumberValue ParseExpression()
    {
        var left = ParseTerm();
        while (true)
        {
            SkipSpaces();
            if (position >= expression.Length) break;
            var op = expression[position];
            if (op != '+' && op != '-') break;
            position++;
            var right = ParseTerm();
            left = op == '+' ? left + right : left - right;
        }
        return left;
    }

    private NumberValue ParseTerm()
    {
        var left = ParseUnary();
        while (true)
        {
            SkipSpaces();
            if (position >= expression.Length) break;
            var op = expression[position];
            if (op != '*' && op != '/' && op != '%') break;
            position++;
            var right = ParseUnary();
            if (op == '*') left = left * right;
            else if (op == '/') left = left / right;
            else left = left % right;
        }
        return left;
    }

    private NumberValue ParseUnary()
    {
        SkipSpaces();
        if (position < expression.Length)
        {
            if (expression[position] == '+') { position++; return NumberValue.UnaryPlus(ParsePower()); }
            if (expression[position] == '-') { position++; return NumberValue.UnaryMinus(ParsePower()); }
        }
        return ParsePower();
    }

    private NumberValue ParsePower()
    {
        var left = ParsePrimary();
        SkipSpaces();
        if (position < expression.Length && expression[position] == '^')
        {
            position++;
            var right = ParseUnary();
            left = NumberValue.Pow(left, right);
        }
        return left;
    }

    private NumberValue ParsePrimary()
    {
        SkipSpaces();
        if (position >= expression.Length)
            throw new Exception("Неожиданный конец выражения");

        if (expression[position] == '(')
        {
            position++;
            var result = ParseExpression();
            SkipSpaces();
            if (position >= expression.Length || expression[position] != ')')
                throw new Exception("Пропущена закрывающая скобка");
            position++;
            return result;
        }

        if (char.IsLetter(expression[position]))
        {
            var start = position;
            while (position < expression.Length && (char.IsLetterOrDigit(expression[position]) || expression[position] == '_'))
                position++;

            var name = expression.Substring(start, position - start);
            var nameLower = name.ToLowerInvariant();

            if (nameLower == "pi") return new NumberValue(Math.PI, false);
            if (nameLower == "e") return new NumberValue(Math.E, false);

            SkipSpaces();
            if (position < expression.Length && expression[position] == '(')
            {
                position++;
                var arguments = new List<NumberValue>();
                SkipSpaces();
                if (position < expression.Length && expression[position] != ')')
                {
                    arguments.Add(ParseExpression());
                    while (true)
                    {
                        SkipSpaces();
                        if (position >= expression.Length || expression[position] != ',') break;
                        position++;
                        arguments.Add(ParseExpression());
                    }
                }
                SkipSpaces();
                if (position >= expression.Length || expression[position] != ')')
                    throw new Exception("Пропущена ')' после аргументов функции");
                position++;
                return EvaluateFunction(nameLower, arguments);
            }

            if (RomanNumerals.TryParse(name, out var romanValue))
            {
                ContainsRoman = true;
                return NumberValue.FromRoman(romanValue);
            }

            throw new Exception($"Неизвестный идентификатор: '{name}'");
        }

        var numberStart = position;
        while (position < expression.Length && (char.IsDigit(expression[position]) || expression[position] == '.'))
            position++;
        if (position < expression.Length && (expression[position] == 'e' || expression[position] == 'E'))
        {
            position++;
            if (position < expression.Length && (expression[position] == '+' || expression[position] == '-'))
                position++;
            while (position < expression.Length && char.IsDigit(expression[position]))
                position++;
        }

        if (numberStart == position)
            throw new Exception($"Ожидалось число на позиции {position}");

        return NumberValue.FromLiteral(expression.Substring(numberStart, position - numberStart));
    }

    private NumberValue EvaluateFunction(string name, List<NumberValue> arguments)
    {
        void CheckArgumentCount(int expectedCount)
        {
            if (arguments.Count != expectedCount)
                throw new Exception($"Функция '{name}' ожидает {expectedCount} аргументов, получено {arguments.Count}");
        }
        switch (name)
        {
            case "abs": CheckArgumentCount(1); return new NumberValue(Math.Abs(arguments[0].Value), arguments[0].IsInteger);
            case "sin": CheckArgumentCount(1); return new NumberValue(Math.Sin(arguments[0].Value), false);
            case "cos": CheckArgumentCount(1); return new NumberValue(Math.Cos(arguments[0].Value), false);
            case "tan": CheckArgumentCount(1); return new NumberValue(Math.Tan(arguments[0].Value), false);
            case "asin": CheckArgumentCount(1); return new NumberValue(Math.Asin(arguments[0].Value), false);
            case "acos": CheckArgumentCount(1); return new NumberValue(Math.Acos(arguments[0].Value), false);
            case "atan": CheckArgumentCount(1); return new NumberValue(Math.Atan(arguments[0].Value), false);
            case "atan2": CheckArgumentCount(2); return new NumberValue(Math.Atan2(arguments[0].Value, arguments[1].Value), false);
            case "sqrt": CheckArgumentCount(1); return new NumberValue(Math.Sqrt(arguments[0].Value), false);
            case "log": CheckArgumentCount(1); return new NumberValue(Math.Log10(arguments[0].Value), false);
            case "log10": CheckArgumentCount(1); return new NumberValue(Math.Log10(arguments[0].Value), false);
            case "ln": CheckArgumentCount(1); return new NumberValue(Math.Log(arguments[0].Value), false);
            case "exp": CheckArgumentCount(1); return new NumberValue(Math.Exp(arguments[0].Value), false);
            case "ceil":
            case "ceiling": CheckArgumentCount(1); return new NumberValue(Math.Ceiling(arguments[0].Value), false);
            case "floor": CheckArgumentCount(1); return new NumberValue(Math.Floor(arguments[0].Value), false);
            case "round": CheckArgumentCount(1); return new NumberValue(Math.Round(arguments[0].Value, MidpointRounding.AwayFromZero), false);
            case "truncate": CheckArgumentCount(1); return new NumberValue(Math.Truncate(arguments[0].Value), false);
            case "sign": CheckArgumentCount(1); return new NumberValue(Math.Sign(arguments[0].Value), true);
            case "pow": CheckArgumentCount(2); return NumberValue.Pow(arguments[0], arguments[1]);
            case "max": CheckArgumentCount(2); return new NumberValue(Math.Max(arguments[0].Value, arguments[1].Value), arguments[0].IsInteger && arguments[1].IsInteger);
            case "min": CheckArgumentCount(2); return new NumberValue(Math.Min(arguments[0].Value, arguments[1].Value), arguments[0].IsInteger && arguments[1].IsInteger);
            case "ieeeremainder": CheckArgumentCount(2); return new NumberValue(Math.IEEERemainder(arguments[0].Value, arguments[1].Value), false);
            default:
                throw new Exception($"Неизвестная функция: {name}");
        }
    }
}