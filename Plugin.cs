using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace NukeLib;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
public class Plugin : BaseUnityPlugin {
    /// <summary>
    /// The instance of the plugin
    /// </summary>
    public static Plugin? Instance;

    // Logger
    internal static ManualLogSource Log;

    // Plugin config
    public static string workingPath = Assembly.GetExecutingAssembly().Location;
    public static string workingDir = Path.GetDirectoryName(workingPath);
    public const string PluginGUID = "com.github.end-4.nukeLib";
    public const string PluginName = "NukeLib";
    public const string PluginVersion = "0.7.0";

    private void Awake() {
        if (Instance != null) return;
        Instance = this;
        Log = Logger;

        Harmony harmony = new Harmony(PluginGUID);
        harmony.PatchAll();
    }
}
