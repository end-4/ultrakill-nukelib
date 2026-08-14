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

    public static bool IsInGame() {
        return IsSafe() &&
               SceneHelper.CurrentScene is not ("Main Menu" or "Level 2-S" or "Intermission1" or "Intermission2");
    }

    public static event Action<Scene, LoadSceneMode> SafeSceneLoaded;
    public static event Action SafeSceneLoadedNoParam;
    public static event Action<Scene, LoadSceneMode> SafeSceneLoadedDelayed;
    public static event Action SafeSceneLoadedDelayedNoParam;

    private static void CheckScene(Scene scene, LoadSceneMode mode) {
        try {
            if (!IsSafe()) return;
            SafeSceneLoaded?.Invoke(scene, mode);
            SafeSceneLoadedNoParam?.Invoke();
            ExecutionUtils.RunNextFrame(() => {
                SafeSceneLoadedDelayed?.Invoke(scene, mode);
                SafeSceneLoadedDelayedNoParam?.Invoke();
            });
        } catch (Exception e) {
            Plugin.Log.LogError($"Error on scene load (most likely caused by mods that depend on NukeLib): {e}");
        }
    }
}
