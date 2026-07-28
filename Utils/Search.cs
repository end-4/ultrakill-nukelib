using System;
using System.Collections.Generic;
using System.Linq;

namespace NukeLib.Utils;

/// <summary>
/// A searchable entry
/// </summary>
public record Searchable {
    /// <summary>
    /// The main text, such as the app name in an application launcher
    /// </summary>
    public string Primary;

    /// <summary>
    /// The secondary texts, such as the description and category in an application launcher
    /// </summary>
    public Dictionary<string, string> Secondaries;
}

/// <summary>
/// Class providing search functionality
/// </summary>
public static class Search {
    // Scoring Weights
    private const int WeightPrimary = 3;
    private const int WeightSecondary = 1;

    private const int ExactMatchBonus = 100;
    private const int PrefixBonus = 50;
    private const int WordBoundaryBonus = 30;
    private const int ConsecutiveMatchBonus = 15;
    private const int CamelCaseBonus = 20;

    private const int UnmatchedGapPenalty = -2;

    /// <summary>
    /// Perform a search against primary and secondary fields.
    /// </summary>
    public static Searchable[] Invoke(string query, Searchable[] items) {
        if (items == null || items.Length == 0) return [];
        if (string.IsNullOrWhiteSpace(query)) return items;

        query = query.Trim();

        var scoredResults = new List<(Searchable Item, int Score)>();

        foreach (var item in items) {
            int bestScore = int.MinValue;

            // Primary
            if (!string.IsNullOrEmpty(item.Primary)) {
                int primaryScore = CalculateFuzzyScore(query, item.Primary);
                if (primaryScore > int.MinValue) {
                    bestScore = Math.Max(bestScore, primaryScore * WeightPrimary);
                }
            }

            // Secondaries
            if (item.Secondaries != null) {
                foreach (var pair in item.Secondaries) {
                    if (string.IsNullOrEmpty(pair.Value)) continue;

                    int secScore = CalculateFuzzyScore(query, pair.Value);
                    if (secScore > int.MinValue) {
                        bestScore = Math.Max(bestScore, secScore * WeightSecondary);
                    }
                }
            }

            // Keep item if it matched at least one field
            if (bestScore > int.MinValue) {
                scoredResults.Add((item, bestScore));
            }
        }

        // Return items sorted by highest score first
        return scoredResults
            .OrderByDescending(r => r.Score)
            .Select(r => r.Item)
            .ToArray();
    }

    /// <summary>
    /// Calculates subsequence alignment score between a query and target string.
    /// Returns int.MinValue if query characters cannot be matched sequentially.
    /// </summary>
    private static int CalculateFuzzyScore(string query, string target) {
        if (string.Equals(query, target, StringComparison.OrdinalIgnoreCase))
            return ExactMatchBonus + 100;

        if (target.StartsWith(query, StringComparison.OrdinalIgnoreCase))
            return PrefixBonus + (query.Length * ConsecutiveMatchBonus);

        int queryIdx = 0;
        int score = 0;
        bool lastWasMatch = false;

        for (int i = 0; i < target.Length && queryIdx < query.Length; i++) {
            char targetChar = target[i];
            char queryChar = query[queryIdx];

            if (char.ToLowerInvariant(targetChar) == char.ToLowerInvariant(queryChar)) {
                queryIdx++;

                // Base match point
                score += 10;

                // Word boundary bonus
                if (i == 0) {
                    score += PrefixBonus;
                } else if (IsWordBoundary(target, i)) {
                    score += WordBoundaryBonus;
                }

                // CamelCase bonus
                if (char.IsUpper(targetChar) && i > 0 && char.IsLower(target[i - 1])) score += CamelCaseBonus;

                // Consecutive char bonus
                if (lastWasMatch) score += ConsecutiveMatchBonus;


                lastWasMatch = true;
            } else {
                // Penalty for skipped characters
                score += UnmatchedGapPenalty;
                lastWasMatch = false;
            }
        }

        // All characters in query must match in order
        return (queryIdx == query.Length) ? score : int.MinValue;
    }

    private static bool IsWordBoundary(string text, int index) {
        if (index == 0) return true;
        char prev = text[index - 1];
        return char.IsWhiteSpace(prev) || char.IsPunctuation(prev) || prev == '_' || prev == '-';
    }
}
