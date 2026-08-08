using System;
using UnityEngine.SceneManagement;

namespace NukeLib.Utils;

public static class SceneUtils {
    static SceneUtils() {
        SceneManager.sceneLoaded += CheckScene;
    }

    public static bool IsSafe() {
        return !(string.IsNullOrEmpty(SceneHelper.CurrentScene) || SceneHelper.CurrentScene == "Bootstrap" ||
                 SceneHelper.CurrentScene == "Intro");
    }

    public static event Action<Scene, LoadSceneMode> SafeSceneLoaded;
    public static event Action SafeSceneLoadedNoParam;

    private static void CheckScene(Scene scene, LoadSceneMode mode) {
        try {
            if (!IsSafe()) return;
            SafeSceneLoaded?.Invoke(scene, mode);
            SafeSceneLoadedNoParam?.Invoke();
        } catch (Exception e) {
            Plugin.Log.LogError($"Error on scene load (most likely caused by mods that depend on NukeLib): {e}");
        }
    }
}
