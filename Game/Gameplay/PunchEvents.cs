using System;
using HarmonyLib;

namespace NukeLib.Game.Gameplay;

/// <summary>
/// Stuff related to punching
/// </summary>
public static class PunchEvents {
    public static event Action OnParry;

    [HarmonyPatch(typeof(Punch))]
    internal static class PunchPatches {
        [HarmonyPostfix]
        [HarmonyPatch("Parry")]
        private static void Parry_Postfix(Punch __instance, EnemyIdentifier eid) {
            OnParry?.Invoke();
        }
    }
}
