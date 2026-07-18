using System.Text;
using System.Text.RegularExpressions;

namespace NukeLib.Utils;

public static class TextUtils {
    public static string ToSnakeCase(this string input) {
        if (string.IsNullOrEmpty(input)) {
            return input;
        }

        string result = Regex.Replace(input, @"(?<!^)(?=[A-Z][a-z])|(?<=[a-z0-9])(?=[A-Z])", "_");
        return result.ToLowerInvariant();
    }

    /// <summary>
    /// Word-wraps text given a limit of chars per line. If a word is longer than the line length, it will not be split
    /// </summary>
    /// <param name="text">The text</param>
    /// <param name="lineLength">Max characters per line</param>
    /// <returns>The wrapped text</returns>
    public static string WrapText(this string text, int lineLength) {
        // Split by spaces to get individual words
        string[] words = text.Split(' ');
        StringBuilder result = new StringBuilder();
        StringBuilder currentLine = new StringBuilder();

        foreach (string word in words) {
            // If adding this word exceeds the limit
            if (currentLine.Length + word.Length > lineLength) {
                // If the current line is empty, it means a single word is longer than lineLength
                if (currentLine.Length == 0) {
                    result.AppendLine(word);
                } else {
                    result.AppendLine(currentLine.ToString().TrimEnd());
                    currentLine.Clear();
                    currentLine.Append(word).Append(" ");
                }
            } else {
                currentLine.Append(word).Append(" ");
            }
        }

        // Add any remaining text
        if (currentLine.Length > 0) {
            result.Append(currentLine.ToString().TrimEnd());
        }

        return result.ToString();
    }
}
