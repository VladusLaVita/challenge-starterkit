using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConsoleApp
{
    public record Point(double X, double Y);

    public class ShapeRecognizer
    {
        private const string _default = "circle";
        private const double _diagonalTolerance = 0.05;
        private const double _angleTolerance = 10.0;
        private const double _sideTolerance = 0.15;

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

            var regex = new Regex(@"\(\s*(-?\d+(?:\.\d+)?)\s*,\s*(-?\d+(?:\.\d+)?)\s*\)");
            var matches = regex.Matches(input);

            foreach (Match match in matches)
            {
                if (match.Groups.Count >= 3)
                {
                    if (double.TryParse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double x) &&
                        double.TryParse(match.Groups[2].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double y))
                    {
                        points.Add(new Point(x, y));
                    }
                }
            }

            return points;
        }

        private string Recognize(List<Point> points)
        {
            var uniquePoints = points.Distinct().ToList();
            if (uniquePoints.Count < 3)
                return _default;

            var hull = ConvexHull(uniquePoints);

            if (hull.Count == 3 && IsEquilateralTriangle(hull))
                return "equilateraltriangle";

            if (hull.Count == 4 && IsSquare(hull))
                return "square";

            if (IsCircle(hull))
                return "circle";

            return _default;
        }

        private bool IsCircle(List<Point> contour)
        {
            if (contour.Count < 6)
                return false;

            var area = CalculatePolygonArea(contour);
            var perimeter = CalculatePolygonPerimeter(contour);
            if (perimeter == 0 || area == 0)
                return false;

            var quota = 4 * Math.PI * area / Math.Pow(perimeter, 2);

            return quota > 0.80;
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

            if (lower.Count > 0) lower.RemoveAt(lower.Count - 1);
            if (upper.Count > 0) upper.RemoveAt(upper.Count - 1);
            lower.AddRange(upper);

            return lower;
        }

        private double Distance(Point a, Point b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        private double Cross(Point o, Point a, Point b)
        {
            return (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);
        }

        private double Angle(Point prev, Point current, Point next)
        {
            var ux = prev.X - current.X;
            var uy = prev.Y - current.Y;
            var vx = next.X - current.X;
            var vy = next.Y - current.Y;

            var dotProduct = ux * vx + uy * vy;
            var modU = Math.Sqrt(ux * ux + uy * uy);
            var modV = Math.Sqrt(vx * vx + vy * vy);

            if (modU == 0 || modV == 0) return 0;

            var cos = dotProduct / (modU * modV);
            cos = Math.Max(-1.0, Math.Min(1.0, cos));

            return Math.Acos(cos) * (180.0 / Math.PI);
        }
    }
}
