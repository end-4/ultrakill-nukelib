using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using GameConsole.pcon;
using NukeLib.Text;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace NukeLib.UI;

/// <summary>
/// A component for setting the enemy icon
/// </summary>
public class EnemyIconController : MonoBehaviour {
    private static bool iconsLoaded = false;
    private static bool vanillaIconsLoaded = false;
    private static readonly string BundlePath = Path.Combine(Plugin.workingDir, "assets", "nukelib_enemies.bundle");
    private static string DEFAULT_ICON = "default";

    /// <summary>
    /// The icons style
    /// </summary>
    public IconStyle style = IconStyle.Simple;

    /// <summary>
    /// Icon style options
    /// </summary>
    public enum IconStyle {
        /// <summary>
        /// Clean vector style optimized for viewing at small scale
        /// </summary>
        Simple,

        /// <summary>
        /// Literal screenshot style used in vanilla spawner arm menu
        /// </summary>
        Vanilla
    };

    private static string[] IconNames = {
        "default",
        "big_johninator",
        "cancerous_rodent",
        "centaur_mortar",
        "centaur_orb",
        "centaur_rocket",
        "cerberus",
        "deathcatcher",
        "drone",
        "ferryman",
        "filth",
        "flesh_panopticon",
        "flesh_prison",
        "gabriel",
        "gabriel_second",
        "gutterman",
        "guttertank",
        "hideous_mass",
        "idol",
        "malicious_face",
        "mandalore",
        "mindflayer",
        "minos_prime",
        "minotaur",
        "mirror_reaper",
        "power",
        "providence",
        "puppet",
        "schism",
        "sisyphus",
        "sisyphus_prime",
        "soldier",
        "stalker",
        "stray",
        "mannequin",
        "streetcleaner",
        "swordsmachine",
        "turret",
        "v2",
        "very_cancerous_rodent",
        "virtue"
    };

    private static Dictionary<string, Sprite> EnemyIcons = new();
    private static IEnumerable<SpawnableObject> vanillaEnemies;

    private static void LoadSimpleIcons() {
        AssetBundle bundle = AssetBundle.LoadFromFile(BundlePath);
        // Load icons
        for (int i = 0; i < IconNames.Length; i++) {
            string iconName = IconNames[i];
            Sprite iconSprite = bundle.LoadAsset<Sprite>(iconName);
            EnemyIcons.Add(iconName, iconSprite);
        }

        bundle.Unload(false);
    }

    private static void LoadVanillaIcons() {
        vanillaEnemies = Resources.FindObjectsOfTypeAll<SpawnableObjectsDatabase>().SelectMany(db => db.enemies);
    }

    public EnemyIdentifier enemyIdentifier;
    public EnemyType enemyType;
    public string enemyName;

    private void Awake() {
        if (style == IconStyle.Simple && !iconsLoaded) {
            LoadSimpleIcons();
            iconsLoaded = true;
        } else if (style == IconStyle.Vanilla && !vanillaIconsLoaded) {
            LoadVanillaIcons();
            vanillaIconsLoaded = vanillaEnemies.Count() > 0;
        }
    }

    private void Start() {
        SetEnemyIcon();
    }

    private static string NormalizeEnemyName(string name) {
        return name.ToLower().Replace(" ", "_");
    }

    private static string GuessIconFromName(string name) {
        if (name.IsNullOrWhiteSpace()) return string.Empty;
        string nameLower = name.ToLower();
        var normalized = NormalizeEnemyName(nameLower);

        if (nameLower.StartsWith("sisyphean")) return "sisyphus";
        if (nameLower.StartsWith("sentry")) return "turret";
        if (nameLower.StartsWith("earthmover mortar")) return "centaur_mortar";
        if (nameLower.StartsWith("earthmover rocket launcher")) return "centaur_rocket";
        if (nameLower.StartsWith("earthmover tower")) return "centaur_orb";
        if (nameLower.StartsWith("<s>minotaur")) return "minotaur";
        if (nameLower.StartsWith("mysterious druid knight")) return "mandalore";
        if (nameLower.StartsWith("???")) return "puppet";
        if (nameLower.StartsWith("minos prime")) return "minos_prime";

        // We loop backwards so longer names get checked first
        for (int i = IconNames.Length - 1; i >= 0; i--) {
            var iconName = IconNames[i];
            if (iconName == "sisyphus") continue;
            var unNormalized = iconName.Replace("_", " ");
            if (nameLower.StartsWith(unNormalized)) return iconName;
        }

        return EnemyIcons.ContainsKey(normalized) ? normalized : string.Empty;
    }

    private void SetEnemyIcon() {
        // Vanilla icons
        if (style == IconStyle.Vanilla) {
            this.gameObject.GetComponent<Image>().sprite = vanillaEnemies.FirstOrDefault(spawnable => {
                var vanillaEid = spawnable.gameObject.GetComponentInChildren<EnemyIdentifier>(true);
                if (enemyIdentifier != null) return vanillaEid?.FullName == enemyIdentifier.FullName;
                return vanillaEid.enemyType == enemyType;
            })?.gridIcon;
            return;
        }

        // Custom icons
        string enemyTypeId = (enemyIdentifier != null) ? enemyIdentifier.enemyType.ToString() : enemyType.ToString();
        string iconName = DEFAULT_ICON;
        string potentialIconName = enemyTypeId.ToSnakeCase();
        if (EnemyIcons.ContainsKey(potentialIconName)) {
            iconName = potentialIconName;
        }
        if (iconName == DEFAULT_ICON) {
            // Guess
            var guessed1 = GuessIconFromName(enemyIdentifier?.FullName.ToLower() ?? "");
            if (!guessed1.IsNullOrWhiteSpace() && EnemyIcons.ContainsKey(guessed1)) iconName = guessed1;
            var guessed2 = GuessIconFromName(enemyName);
            if (!guessed2.IsNullOrWhiteSpace() && EnemyIcons.ContainsKey(guessed2)) iconName = guessed2;
        }

        if (EnemyIcons.TryGetValue(iconName, out Sprite icon)) {
            this.gameObject.GetComponent<Image>().sprite = icon;
        }
    }
}
