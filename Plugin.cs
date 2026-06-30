using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;

namespace NukeLib;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
public class Plugin : BaseUnityPlugin {
    // Logger
    internal static ManualLogSource Log;

    // Plugin config
    public static string workingPath = Assembly.GetExecutingAssembly().Location;
    public static string workingDir = Path.GetDirectoryName(workingPath);
    public const string PluginGUID = "com.github.end-4.nukeLib";
    public const string PluginName = "NukeLib";
    public const string PluginVersion = "0.3.0";

    private void Awake() {
        Log = Logger;
    }
}
