using UnityEngine;

namespace NukeLib.Utils;

/// <summary>
/// Helper for some color stuff
/// </summary>
public static class ColorUtils {
    /// <summary>
    /// Calculates perceived lightness of a color.
    /// Use this as a value to inform adjustments, rather than a concrete lightness value.
    /// </summary>
    /// <param name="color">The color</param>
    /// <returns>The lightness</returns>
    public static float PerceivedLightness(this Color color) {
        // https://www.w3.org/TR/AERT/#color-contrast
        return (0.299f * color.r + 0.587f * color.g + 0.114f * color.b);
    }

    /// <summary>
    /// Makes the color (semi) transparent
    /// </summary>
    /// <param name="color">The color</param>
    /// <param name="value">How much to transparentize. 0 = original, 1 = fully transparent</param>
    /// <returns></returns>
    public static Color Transparentize(this Color color, float value = 1) {
        return new Color(color.r, color.g, color.b, color.a * (1 - value));
    }

    /// <summary>
    /// Gets R, G, B, A values from a Color
    /// </summary>
    /// <param name="color">The color</param>
    /// <returns>A float array containing the four values</returns>
    public static float[] GetValues(this Color color) {
        return [color.r, color.g, color.b, color.a];
    }
}
