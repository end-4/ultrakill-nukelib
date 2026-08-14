using System;
using HarmonyLib;

namespace NukeLib.Game;

public static class CheckpointEvents {
    /// <summary>
    /// Triggered when the player restarts at a checkpoint
    /// </summary>
    public static event Action? CheckpointLoadedNoParam;

    /// <summary>
    /// Triggered when the player restarts at a checkpoint
    /// The Action argument is that checkpoint
    /// </summary>
    public static event Action<CheckPoint>? CheckpointLoaded;

    internal static void TriggerCheckpointRestart(CheckPoint checkpoint) {
        CheckpointLoadedNoParam?.Invoke();
        CheckpointLoaded?.Invoke(checkpoint);
    }

    [HarmonyPatch(typeof(CheckPoint))]
    internal static class CheckPointPatches {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(CheckPoint.OnRespawn))]
        public static void OnRespawn_Postfix(CheckPoint __instance) {
            try {
                TriggerCheckpointRestart(__instance);
            } catch (Exception e) {
                Plugin.Log.LogError(e);
            }
        }
    }
}
