using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConsoleApp;

public static class DeterminantSolver
{
    public static bool IsDeterminantTask(string question, string userHint)
    {
        if (string.IsNullOrEmpty(question))
            return false;

        string lowerQuestion = question.ToLower();
        string lowerHint = userHint.ToLower();

        bool containsMatrix = lowerQuestion.Contains("матриц") ||
                             lowerQuestion.Contains("matrix") ||
                             lowerQuestion.Contains("определител") ||
                             lowerQuestion.Contains("determinant") ||
                             lowerHint.Contains("матриц") ||
                             lowerHint.Contains("matrix") ||
                             lowerHint.Contains("определител") ||
                             lowerHint.Contains("determinant");

        bool containsNumbers = Regex.IsMatch(question, @"[-\d]+\s*&\s*[-\d]+\s*&\s*[-\d]+");

        return containsMatrix && containsNumbers;
    }

    public static string Solve(string question)
    {
        double[,] matrix = ParseMatrix(question);
        double determinant = CalculateDeterminant(matrix);

        if (Math.Abs(determinant - Math.Round(determinant)) < 1e-10)
            return Math.Round(determinant).ToString();

        return determinant.ToString("F10").TrimEnd('0').TrimEnd('.');
    }

    private static double[,] ParseMatrix(string question)
    {
        string clean = question;
        clean = Regex.Replace(clean, @"\\begin\{[^}]*\}", "");
        clean = Regex.Replace(clean, @"\\end\{[^}]*\}", "");
        clean = Regex.Replace(clean, @"\\right", "");
        clean = Regex.Replace(clean, @"\\left", "");

        string[] rows = clean.Split(new[] { "\\\\" }, StringSplitOptions.RemoveEmptyEntries);

        if (rows.Length == 1)
        {
            rows = clean.Split(new[] { " \\ ", " \\\\ ", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        }

        List<List<double>> matrixList = new List<List<double>>();

        foreach (var row in rows)
        {
            string cleanedRow = row.Trim();
            if (string.IsNullOrEmpty(cleanedRow)) continue;

            cleanedRow = cleanedRow.Replace("(", "").Replace(")", "").Trim();

            string[] values = cleanedRow.Split('&', StringSplitOptions.RemoveEmptyEntries);

            List<double> rowValues = new List<double>();
            foreach (var val in values)
            {
                string trimmed = val.Trim();
                if (double.TryParse(trimmed, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double number))
                {
                    rowValues.Add(number);
                }
            }

            if (rowValues.Count > 0)
            {
                matrixList.Add(rowValues);
            }
        }

        int size = matrixList.Count;
        if (size == 0) return new double[1, 1];

        var matrix = new double[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < Math.Min(size, matrixList[i].Count); j++)
            {
                matrix[i, j] = matrixList[i][j];
            }
        }

        return matrix;
    }

    private static double CalculateDeterminant(double[,] matrix)
    {
        int n = matrix.GetLength(0);
        if (n == 1) return matrix[0, 0];

        double[,] m = (double[,])matrix.Clone();
        double det = 1.0;

        for (int i = 0; i < n; i++)
        {
            int maxRow = i;
            for (int k = i + 1; k < n; k++)
            {
                if (Math.Abs(m[k, i]) > Math.Abs(m[maxRow, i]))
                {
                    maxRow = k;
                }
            }

            if (Math.Abs(m[maxRow, i]) < 1e-10)
            {
                return 0;
            }

            if (maxRow != i)
            {
                for (int j = 0; j < n; j++)
                {
                    double temp = m[i, j];
                    m[i, j] = m[maxRow, j];
                    m[maxRow, j] = temp;
                }
                det = -det;
            }

            for (int k = i + 1; k < n; k++)
            {
                double factor = m[k, i] / m[i, i];
                for (int j = i; j < n; j++)
                {
                    m[k, j] -= factor * m[i, j];
                }
            }
        }

        for (int i = 0; i < n; i++)
        {
            det *= m[i, i];
        }

        return det;
    }
}