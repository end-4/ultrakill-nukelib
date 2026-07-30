using System;
using System.Text;
using System.Text.RegularExpressions;

namespace NukeLib.Text;

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
        if (string.IsNullOrEmpty(text) || lineLength <= 0)
            return text;

        // Split by newline
        string[] rawLines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        StringBuilder result = new StringBuilder();

        for (int l = 0; l < rawLines.Length; l++) {
            string line = rawLines[l];
            string[] tokens = Regex.Split(line, @"(\s+)");

            StringBuilder currentLine = new StringBuilder();

            foreach (string token in tokens) {
                if (string.IsNullOrEmpty(token)) continue;

                if (currentLine.Length + token.Length > lineLength) {
                    if (currentLine.Length > 0) {
                        result.AppendLine(currentLine.ToString().TrimEnd());
                        currentLine.Clear();

                        // Skip leading whitespace
                        if (char.IsWhiteSpace(token[0])) continue;
                    }

                    // Longer than lineLength -> just add it
                    if (token.Length >= lineLength) {
                        result.AppendLine(token);
                        continue;
                    }
                }

                currentLine.Append(token);
            }

            // Last line
            if (currentLine.Length > 0) {
                result.Append(currentLine.ToString().TrimEnd());
            }

            // Reconstruct lines
            if (l < rawLines.Length - 1) {
                result.AppendLine();
            }
        }

        return result.ToString();
    }
}
