using System;
using System.Text;

namespace ConsoleApp;

public class Cypher
{
    private static readonly string Alphabet = " '0123456789abcdefghijklmnopqrstuvwxyz";

    public static string Solve(string question)
    {
        if (string.IsNullOrWhiteSpace(question))
            return string.Empty;

        var parts = question.Split('#', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
            return string.Empty;

        var config = parts[0];
        var encryptedText = parts[1];

        var shiftPart = config.Split('=');
        if (shiftPart.Length < 2 || !int.TryParse(shiftPart[1], out int shift))
            return string.Empty;

        var result = new StringBuilder();
        int alphabetLength = Alphabet.Length;

        foreach (var c in encryptedText)
        {
            int index = Alphabet.IndexOf(c);
            if (index == -1)
            {
                result.Append(c);
                continue;
            }

            // Сдвигаем влево для дешифрования
            int newIndex = (index - shift) % alphabetLength;
            if (newIndex < 0)
                newIndex += alphabetLength;

            result.Append(Alphabet[newIndex]);
        }

        return result.ToString();
    }
}
