using System;
using System.IO;
using System.Text.RegularExpressions;


namespace Ceg.ConsoleApp.Extensions
{
    public static class StringExtensions
    {
        public static string ReplaceEx(this string input, string pattern, string replacement)
        {
            return Regex.Replace(input, pattern, replacement);
        }

        public static string ReplaceEx(this string input, string pattern, MatchEvaluator evaluator)
        {
            return Regex.Replace(input, pattern, evaluator);
        }

        public static void EnsureDirectoryExists(this string filePath)
        {
            var dirName = Path.GetDirectoryName(filePath);

            if (string.IsNullOrEmpty(dirName))
            {
                throw new ArgumentException("dirName");
            }

            Directory.CreateDirectory(dirName);
        }
    }
}
