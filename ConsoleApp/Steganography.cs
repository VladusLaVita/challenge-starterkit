using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp
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

            if (lines.Count == 0)
                return "";

            var romanNumeral = lines[0];
            var position = RomanToInt(romanNumeral);
            var textLines = lines.Skip(1).ToList();
            var message = new List<char>();

            foreach (var line in textLines)
            {
                if (position > 0 && position <= line.Length)
                {
                    char c = line[position - 1];
                    message.Add(c);
                }
            }

            var result = new string(message.ToArray());
            result = result.Trim();

            return result;
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