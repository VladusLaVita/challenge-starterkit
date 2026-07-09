using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConsoleApp1
{
    public class Steganography
    {
        public static string Solve(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return "";

            var lines = question.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                               .Select(l => l.Trim())
                               .Where(l => !string.IsNullOrEmpty(l))
                               .ToList();

            if (lines.Count < 2)
                return "";

            var firstLine = lines[0];
            var contentLines = lines.Skip(1).ToList();
            if (IsRomanNumeral(firstLine))
                return SolveByRoman(firstLine, contentLines);
            else
            {
                var indicesLine = firstLine;
                var fullText = string.Join(" ", contentLines);

                return SolveByIndices(indicesLine, fullText);
            }
        }

        private static bool IsRomanNumeral(string s)
            => !string.IsNullOrEmpty(s) && Regex.IsMatch(s, @"^[IVXLCDM]+$");


        private static string SolveByRoman(string roman, List<string> lines)
        {
            var position = RomanToInt(roman);
            var message = new List<char>();

            foreach (var line in lines)
            {
                if (position > 0 && position <= line.Length)
                {
                    char c = line[position - 1];
                    message.Add(c);
                }
                else
                    message.Add(' ');
            }

            var result = new string(message.ToArray());

            return result.Trim();
        }

        private static string SolveByIndices(string indicesLine, string text)
        {
            var indices = new List<int>();
            foreach (System.Text.RegularExpressions.Match match in Regex.Matches(indicesLine, @"\d+"))
            {
                if (int.TryParse(match.Value, out int num))
                    indices.Add(num);
            }

            var message = new List<char>();
            foreach (var index in indices)
            {
                if (index > 0 && index <= text.Length)
                    message.Add(text[index - 1]);
                else
                    message.Add(' ');
            }

            return new string(message.ToArray()).Trim();
        }

        private static int RomanToInt(string roman)
        {
            var romanValues = new Dictionary<char, int>
            {
                {'I', 1},
                {'V', 5},
                {'X', 10},
                {'L', 50},
                {'C', 100},
                {'D', 500},
                {'M', 1000}
            };

            var result = 0;
            for (int i = 0; i < roman.Length; i++)
            {
                if (i + 1 < roman.Length && romanValues[roman[i]] < romanValues[roman[i + 1]])
                    result -= romanValues[roman[i]];
                else
                    result += romanValues[roman[i]];
            }

            return result;
        }
    }
}