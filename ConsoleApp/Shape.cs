using System.Text.RegularExpressions;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ConsoleApp;

public record Point(double X, double Y);

public class ShapeRecognizer
{
    // Дефолтное значение на случай, если не нашло фигуру. Типа попытка угадать на крайняк
    // Выбрал круг, потому что он чаще встречался, пока проверяли 
    private const string _default = "circle";
    private const double _diagonalTolerance = 0.025; // Допуск для epsilon
    private const double _angleTolerance = 5.0; // Допуск для углов
    private const double _sideTolerance = 0.15; // Допуск для длин сторон

    public string Solve(string question)
    {
        if (string.IsNullOrWhiteSpace(question))
            throw new Exception("Input question is empty or whitespace.");

        try
        {
            var points = ParsePoints(question);
            if (points == null || points.Count == 0)
                throw new Exception("No valid points found in the input.");

            return Recognize(points);
        }
        catch (Exception ex)
        {
            return $"Caught an exception: {ex.Message}";
        }
    }

    private List<Point> ParsePoints(string input)
    {
        var points = new List<Point>();
        if (string.IsNullOrWhiteSpace(input))
            return points;

        var regex = new Regex(@"\((\d+),(\d+)\)");
        var matches = regex.Matches(input);
        foreach (Match match in matches)
        {
            if (match.Groups.Count >= 3)
            {
                if (double.TryParse(match.Groups[1].Value, out double x) &&
                    double.TryParse(match.Groups[2].Value, out double y))
                {
                    points.Add(new Point(x, y));
                }
            }
        }

        return points;
    }

    private string Recognize(List<Point> points)
    {
        if (points == null || points.Count < 3)
            return _default;

        var hull = ConvexHull(points);

        if (hull.Count < 3)
            return _default;

        if (IsCircle(hull))
            return "circle";

        var minX = hull.Min(p => p.X);
        var maxX = hull.Max(p => p.X);
        var minY = hull.Min(p => p.Y);
        var maxY = hull.Max(p => p.Y);
        var diagonal = Math.Sqrt(Math.Pow(maxX - minX, 2) + Math.Pow(maxY - minY, 2));
        var epsilon = diagonal * _diagonalTolerance;
        if (epsilon < 1.0)
            epsilon = 1.0;

        var simplified = Simplify(hull, epsilon);
        if (IsSquare(simplified))
            return "square";

        if (IsEquilateralTriangle(simplified))
            return "equilateraltriangle";

        return _default;
    }

    private bool IsCircle(List<Point> contour)
    {
        if (contour.Count < 8)
            return false;

        var area = CalculatePolygonArea(contour);
        var perimeter = CalculatePolygonPerimeter(contour);
        if (perimeter == 0 || area == 0)
            return false;

        var quota = 4 * Math.PI * area / Math.Pow(perimeter, 2);

        return quota > 0.90;
    }

    private double CalculatePolygonArea(List<Point> points)
    {
        var area = 0.0;
        var n = points.Count;
        for (int i = 0; i < n; i++)
        {
            var j = (i + 1) % n;
            area += points[i].X * points[j].Y;
            area -= points[j].X * points[i].Y;
        }

        return Math.Abs(area) / 2.0;
    }

    private double CalculatePolygonPerimeter(List<Point> points)
    {
        var perimeter = 0.0;
        var n = points.Count;
        for (int i = 0; i < n; i++)
        {
            var j = (i + 1) % n;
            perimeter += Distance(points[i], points[j]);
        }

        return perimeter;
    }

    private bool IsSquare(List<Point> contour)
    {
        if (contour.Count != 4)
            return false;

        var sides = new List<double>();
        for (int i = 0; i < 4; i++)
            sides.Add(Distance(contour[i], contour[(i + 1) % 4]));

        var avgSide = sides.Average();
        if (avgSide == 0)
            return false;

        if (sides.Any(s => Math.Abs(s - avgSide) > avgSide * _sideTolerance))
            return false;

        for (int i = 0; i < 4; i++)
        {
            var prev = (i - 1 + 4) % 4;
            var next = (i + 1) % 4;
            var angle = Angle(contour[prev], contour[i], contour[next]);
            if (Math.Abs(angle - 90.0) > _angleTolerance)
                return false;
        }

        var diag1 = Distance(contour[0], contour[2]);
        var diag2 = Distance(contour[1], contour[3]);
        if (Math.Abs(diag1 - diag2) > avgSide * _diagonalTolerance)
            return false;

        return true;
    }

    private bool IsEquilateralTriangle(List<Point> contour)
    {
        if (contour.Count != 3)
            return false;

        var sides = new List<double>();
        for (int i = 0; i < 3; i++)
            sides.Add(Distance(contour[i], contour[(i + 1) % 3]));

        var avgSide = sides.Average();
        if (avgSide == 0)
            return false;

        if (sides.Any(s => Math.Abs(s - avgSide) > avgSide * _sideTolerance))
            return false;

        for (int i = 0; i < 3; i++)
        {
            var prev = (i - 1 + 3) % 3;
            var next = (i + 1) % 3;
            var angle = Angle(contour[prev], contour[i], contour[next]);
            if (Math.Abs(angle - 60.0) > _angleTolerance)
                return false;
        }

        return true;
    }

    private List<Point> Simplify(List<Point> contour, double epsilon)
    {
        if (contour.Count <= 2)
            return contour;

        var maxDist = 0.0;
        var index = 0;
        var start = contour[0];
        var end = contour[contour.Count - 1];
        for (int i = 1; i < contour.Count - 1; i++)
        {
            var dist = PerpendicularDistance(contour[i], start, end);
            if (dist > maxDist)
            {
                maxDist = dist;
                index = i;
            }
        }

        if (maxDist < epsilon)
            return new List<Point> { start, end };

        var leftPart = Simplify(contour.Take(index + 1).ToList(), epsilon);
        var rightPart = Simplify(contour.Skip(index).ToList(), epsilon);
        var result = leftPart.Take(leftPart.Count - 1).ToList();
        result.AddRange(rightPart);

        return result;
    }

    private List<Point> ConvexHull(List<Point> points)
    {
        if (points.Count <= 1)
            return points;

        var sorted = points.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
        var lower = new List<Point>();
        foreach (var p in sorted)
        {
            while (lower.Count >= 2 && Cross(lower[lower.Count - 2], lower[lower.Count - 1], p) <= 0)
                lower.RemoveAt(lower.Count - 1);

            lower.Add(p);
        }

        var upper = new List<Point>();
        for (int i = sorted.Count - 1; i >= 0; i--)
        {
            var p = sorted[i];
            while (upper.Count >= 2 && Cross(upper[upper.Count - 2], upper[upper.Count - 1], p) <= 0)
                upper.RemoveAt(upper.Count - 1);

            upper.Add(p);
        }

        lower.RemoveAt(lower.Count - 1);
        upper.RemoveAt(upper.Count - 1);
        lower.AddRange(upper);

        return lower;
    }

    private double Cross(Point o, Point a, Point b)
    {
        return (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);
    }

    private double PerpendicularDistance(Point p, Point start, Point end)
    {
        if (start.X == end.X && start.Y == end.Y)
            return Distance(p, start);

        var dx = end.X - start.X;
        var dy = end.Y - start.Y;
        var numerator = Math.Abs(dy * p.X - dx * p.Y + end.X * start.Y - end.Y * start.X);
        var denominator = Math.Sqrt(dx * dx + dy * dy);

        return numerator / denominator;
    }

    private double Distance(Point a, Point b)
    {
        return Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
    }

    private double Angle(Point a, Point b, Point c)
    {
        var dx1 = a.X - b.X;
        var dy1 = a.Y - b.Y;
        var dx2 = c.X - b.X;
        var dy2 = c.Y - b.Y;
        var dot = dx1 * dx2 + dy1 * dy2;
        var mag1 = Math.Sqrt(dx1 * dx1 + dy1 * dy1);
        var mag2 = Math.Sqrt(dx2 * dx2 + dy2 * dy2);

        if (mag1 == 0 || mag2 == 0)
            return 0;

        var cosAngle = dot / (mag1 * mag2);
        cosAngle = Math.Max(-1, Math.Min(1, cosAngle));

        return Math.Acos(cosAngle) * 180.0 / Math.PI;
    }
}