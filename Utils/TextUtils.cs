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

    /// <summary>
    /// Adds a tag to a certain string. Examples:
    /// * Tag("Schism", "Duped") -> "Schism [Duped]"
    /// * Tag("Schism [Duped] [Resized]", "Duped") -> "Schism [Duped] [Resized]"
    /// * Tag("Schism [Duped] [Resized]", "Supercharged") -> "Schism [Duped] [Resized] [Supercharged]"
    /// </summary>
    /// <param name="text">The base text</param>
    /// <param name="tag">The tag name, without square brackets</param>
    /// <returns>The base text with the tag</returns>
    public static string Tag(this string text, string tag) {
        if (string.IsNullOrWhiteSpace(text)) return $"[{tag}]"; // Edging
        string pattern = $@"\[\s*{Regex.Escape(tag)}\s*\]";
        if (Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase)) {
            return text;
        }

        return $"{text.Trim()} [{tag}]";
    }

    /// <summary>
    /// Removes a tag from a certain string. Examples:
    /// * Untag("Schism [Duped] [Resized] [Supercharged]", "Duped") -> "Schism [Resized] [Supercharged]"
    /// * Untag("Schism [Duped] [Resized]", "Arson") -> "Schism [Duped] [Resized]"
    /// </summary>
    /// <param name="text">The text that might have the tag</param>
    /// <param name="tag">The tag name, without square brackets</param>
    /// <returns>The text without the tag</returns>
    public static string Untag(this string text, string tag) {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        string pattern = $@"\s*\[\s*{Regex.Escape(tag)}\s*\]";
        string result = Regex.Replace(text, pattern, string.Empty, RegexOptions.IgnoreCase);
        return Regex.Replace(result.Trim(), @"\s+", " ");
    }

    /// <summary>
    /// Check whether a string has a certain tag
    /// </summary>
    /// <param name="text">The string</param>
    /// <param name="tag">The tag to check, without square brackets</param>
    /// <returns>True if the tag is in the string, false otherwise</returns>
    public static bool HasTag(this string text, string tag) {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(tag))
            return false;

        string pattern = $@"\[\s*{Regex.Escape(tag)}\s*\]";
        return Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase);
    }
}
