using System;
using HarmonyLib;
using UnityEngine;

namespace NukeLib.Game;

/// <summary>
/// Class that helps with style stuff
/// </summary>
public static class StyleHelper {
    /// <summary>
    /// Event emitted when style points are added
    /// </summary>
    public static event Action<StylePointEvent> StylePointAdded;

    [HarmonyPatch(typeof(StyleHUD))]
    internal static class EnemyPatches {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(StyleHUD.AddPoints))]
        private static void AddPoints_Postfix(StyleHUD __instance, int points, string pointID, GameObject sourceWeapon,
            EnemyIdentifier eid, int count, string prefix, string postfix) {
            try {
                StylePointAdded?.Invoke(new StylePointEvent(points, pointID, count, prefix, postfix));
            } catch (Exception e) {
                Plugin.Log.LogError(e);
            }
        }
    }

    /// <summary>
    /// Get formatted style bonus text for a style point event
    /// </summary>
    /// <param name="stylePointEvent">The style point event</param>
    /// <returns>The formatted string</returns>
    public static string GetFormattedString(this StylePointEvent stylePointEvent) {
        var shud = StyleHUD.Instance;
        string pointName = shud?.GetLocalizedName(stylePointEvent.pointID) ?? stylePointEvent.pointID;
        string countSuffix = stylePointEvent.count > 0 ? $" x{stylePointEvent.count}" : string.Empty;
        return stylePointEvent.prefix + pointName + stylePointEvent.postfix + countSuffix;
    }
}

/// <summary>
/// Arguments for the StylePointAdded event
/// </summary>
public struct StylePointEvent {
    /// <summary>
    /// Points gained from the style point addition
    /// </summary>
    public int points { get; }

    /// <summary>
    /// The ID of the style bonus
    /// </summary>
    public string pointID { get; }

    /// <summary>
    /// Number for the style bonus, if applicable > 0 else == -1
    /// </summary>
    public int count { get; }

    /// <summary>
    /// The prefix for the style bonus string
    /// </summary>
    public string prefix { get; }

    /// <summary>
    /// The suffix for the style bonus string
    /// </summary>
    public string postfix { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="points">Points gained from the style point addition</param>
    /// <param name="pointID">The ID of the style bonus</param>
    /// <param name="count">Number for the style bonus, if applicable > 0 else == -1</param>
    /// <param name="prefix">The prefix for the style bonus string</param>
    /// <param name="postfix">The suffix for the style bonus string</param>
    public StylePointEvent(int points, string pointID, int count, string prefix, string postfix) {
        this.points = points;
        this.pointID = pointID;
        this.count = count;
        this.prefix = prefix;
        this.postfix = postfix;
    }
}
