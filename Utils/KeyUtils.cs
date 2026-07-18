using System.Linq;
using UnityEngine;

namespace NukeLib.Utils;

public static class KeyUtils {
    public static readonly KeyCode[] ModifierKeys = [
        KeyCode.LeftControl,
        KeyCode.RightControl,
        KeyCode.LeftShift,
        KeyCode.RightShift,
        KeyCode.LeftAlt,
        KeyCode.RightAlt,
        KeyCode.LeftCommand,
        KeyCode.RightCommand,
        KeyCode.LeftApple,
        KeyCode.RightApple,
        KeyCode.LeftWindows,
        KeyCode.RightWindows,
    ];

    public static bool IsModifier(this KeyCode key) {
        return ModifierKeys.Contains(key);
    }
}
