using System.Collections.Generic;
using System.IO;
using GameConsole.pcon;
using NukeLib.Text;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace NukeLib.UI;

public class EnemyIconController : MonoBehaviour {
    private static bool iconsLoaded = false;
    private static readonly string BundlePath = Path.Combine(Plugin.workingDir, "assets", "nukelib_enemies.bundle");
    private static string DEFAULT_ICON = "default";

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

    private void LoadIcons() {
        AssetBundle bundle = AssetBundle.LoadFromFile(BundlePath);
        // Load icons
        for (int i = 0; i < IconNames.Length; i++) {
            string iconName = IconNames[i];
            Sprite iconSprite = bundle.LoadAsset<Sprite>(iconName);
            EnemyIcons.Add(iconName, iconSprite);
        }

        bundle.Unload(false);
    }


    public EnemyIdentifier enemyIdentifier;

    private void Awake() {
        if (!iconsLoaded) {
            LoadIcons();
            iconsLoaded = true;
        }
    }

    private void Start() {
        SetEnemyIcon(enemyIdentifier);
    }

    private void SetEnemyIcon(EnemyIdentifier enemyIdentifier) {
        string enemyTypeId = enemyIdentifier.enemyType.ToString();
        string iconName = DEFAULT_ICON;
        string potentialIconName = TextUtils.ToSnakeCase(enemyTypeId);
        if (EnemyIcons.ContainsKey(potentialIconName)) {
            iconName = potentialIconName;
        } else {
            string fullNameLower = enemyIdentifier.FullName.ToLower();
            // Plugin.Log.LogInfo($"full name {fullName}");
            if (fullNameLower == "earthmover mortar") potentialIconName = "centaur_mortar";
            else if (fullNameLower == "earthmover rocket launcher") potentialIconName = "centaur_rocket";
            else if (fullNameLower == "earthmover tower") potentialIconName = "centaur_orb";
            else if (fullNameLower == "cancerous rodent") potentialIconName = "cancerous_rodent";
            else if (fullNameLower == "very cancerous rodent") potentialIconName = "very_cancerous_rodent";
            else if (fullNameLower == "big johninator") potentialIconName = "big_johninator";
            if (EnemyIcons.ContainsKey(potentialIconName)) {
                iconName = potentialIconName;
            }
        }

        if (EnemyIcons.TryGetValue(iconName, out Sprite icon)) {
            this.gameObject.GetComponent<Image>().sprite = icon;
        }
    }
}
