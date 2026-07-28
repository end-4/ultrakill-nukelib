using UnityEngine;

namespace NukeLib.Game.Controls;

/// <summary>
/// For pausing
/// </summary>
public static class Pauser {
    /// <summary>
    /// Pause the game
    /// </summary>
    /// <param name="paused">true = menu, false = game</param>
    /// <param name="pauseKey">A unique key for your pausing</param>
    public static void Pause(bool paused, string pauseKey) {
        if (paused) {
            GameState pauseState = new GameState(pauseKey);
            pauseState.cursorLock = LockMode.Unlock;
            pauseState.playerInputLock = LockMode.Lock;
            pauseState.cameraInputLock = LockMode.Lock;
            pauseState.timerModifier = 0;
            pauseState.priority = 69; // heeheeha
            GameStateManager.Instance.RegisterState(pauseState);
            Time.timeScale = 0f;
        } else {
            GameStateManager.Instance.PopState(pauseKey);
            // Edge case: un-pausing on title screen is very weird
            if (SceneHelper.CurrentScene != "Main Menu") {
                // We rely on OptionsManager because it handles time scale more correctly than just setting to 1. I think.
                // And some subtleties I couldn't figure out
                OptionsManager.Instance?.UnPause();
            }
        }
    }
}
