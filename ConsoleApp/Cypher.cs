using System;

namespace ConsoleApp;

public static class Cypher
{
    public static string Solve(string question)
    {
        if (string.IsNullOrWhiteSpace(question)) return string.Empty;

        int firstHash = question.IndexOf('#');
        int secondHash = question.IndexOf('#', firstHash + 1);
        int thirdHash = question.IndexOf('#', secondHash + 1);

        if (firstHash == -1 || secondHash == -1 || thirdHash == -1)
        {
            return question;
        }

        int startIndex = secondHash + 1;
        int length = thirdHash - startIndex;
        string cipherText = question.Substring(startIndex, length);

        char[] charArray = cipherText.ToCharArray();
        Array.Reverse(charArray);

        return new string(charArray);
    }
}
