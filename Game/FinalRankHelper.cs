using System.Collections.Generic;
using HarmonyLib;
using NukeLib.Utils;

namespace NukeLib.Game;

public static class FinalRankHelper {
    public static List<string> ExtraInfoLines = [];

    static FinalRankHelper() {
        SceneUtils.SafeSceneLoadedNoParam += ResetInfo;
    }

    private static void ResetInfo() {
        ExtraInfoLines.Clear();
    }

    public static void AddInfoLine(string line) {
        if (ExtraInfoLines.Contains(line)) return;
        ExtraInfoLines.Add(line);
    }

    public static void RemoveInfoLine(string line) {
        int index = ExtraInfoLines.IndexOf(line);
        if (index != -1) ExtraInfoLines.RemoveAt(index);
    }

    [HarmonyPatch(typeof(FinalRank))]
    internal static class EnemyPatches {
        [HarmonyPostfix]
        [HarmonyPatch("SetInfo")]
        private static void SetInfo_Postfix(FinalRank __instance) {
            string joined = ExtraInfoLines.Join(delimiter: "\n") + "\n";
            __instance.extraInfo.text += joined;
        }
    }
}
