using System;
using HarmonyLib;

namespace NukeLib.Game;

/// <summary>
/// Stuff related to enemies
/// </summary>
public static class EnemyEvents {
    /// <summary>
    /// Event fired when an enemy takes damage. EnemyIdentifier is the enemy, float is the damage
    /// </summary>
    public static event Action<EnemyIdentifier, float> OnDamageTaken;

    /// <summary>
    /// Event fired when an enemy is spawned. EnemyIdentifier is the enemy
    /// </summary>
    public static event Action<EnemyIdentifier> OnSpawn;

    [HarmonyPatch(typeof(EnemyIdentifier))]
    internal static class EnemyPatches {
        [HarmonyPrefix]
        [HarmonyPatch("DeliverDamage")]
        private static void DeliverDamage_Prefix(EnemyIdentifier __instance, out float __state) {
            __state = __instance.health;
        }

        [HarmonyPostfix]
        [HarmonyPatch("DeliverDamage")]
        private static void DeliverDamage_Postfix(EnemyIdentifier __instance, float __state) {
            float newHealth = __instance.health;
            newHealth = Math.Max(newHealth, 0);

            float damage = __state - newHealth;
            // iirc idols have 999 health. idk if it matters but that amount of damage is unrealistic anyway
            if (damage is > 0 and < 998) OnDamageTaken?.Invoke(__instance, damage);
        }

        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void Start_Postfix(EnemyIdentifier __instance) {
            try {
                OnSpawn?.Invoke(__instance);
            } catch (Exception e) {
                Plugin.Log.LogError($"EnemyIdentifier Start postfix failed: {e}");
            }
        }
    }
}
