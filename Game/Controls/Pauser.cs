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
    public static void Pause(bool paused) {
        if (NewMovement.Instance != null) NewMovement.Instance.enabled = !paused; // No moving
        if (GunControl.Instance != null) GunControl.Instance.activated = !paused; // No shooting
        if (CameraController.Instance != null) CameraController.Instance.enabled = !paused; // No looking
        Time.timeScale = paused ? 0f : 1f; // Because of course
    }
}
