using System;
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
    /// <returns>The lightness in range [0, 1]</returns>
    public static float PerceivedLightness(this Color color) {
        // https://www.w3.org/TR/AERT/#color-contrast
        return (0.299f * color.r + 0.587f * color.g + 0.114f * color.b);
    }

    /// <summary>
    /// Makes the color (semi) transparent
    /// </summary>
    /// <param name="color">The color</param>
    /// <param name="value">How much to transparentize. 0 = original, 1 = fully transparent. Default = 1.</param>
    /// <returns>The transparentized color</returns>
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

    private static readonly float MeaningfulColorDiffThreshold = 0.0039f;

    /// <summary>
    /// Check if two colors are the same in hex (#RRGGBBAA) representation
    /// </summary>
    /// <param name="color">First color</param>
    /// <param name="other">Second color</param>
    /// <returns>true if they're the same color in , false otherwise</returns>
    public static bool Approximately(this Color color, Color other) {
        return Mathf.Abs(color.r - other.r) < MeaningfulColorDiffThreshold
               && Mathf.Abs(color.g - other.g) < MeaningfulColorDiffThreshold
               && Mathf.Abs(color.b - other.b) < MeaningfulColorDiffThreshold
               && Mathf.Abs(color.a - other.a) < MeaningfulColorDiffThreshold;
    }

    /// <summary>
    /// Gets the game's color of a weapon variant. 0 = Blue, 1 = Green, 2 = Red, 3 = Gold
    /// </summary>
    /// <param name="variantIndex">The variant number</param>
    /// <returns>The color for the variant</returns>
    public static Color GetWeaponVariantColor(int variantIndex) {
        var cols = ColorBlindSettings.Instance?.variationColors;
        return cols == null ? Color.white : cols[variantIndex];
    }

    /// <summary>
    /// Gets a color that is safe to be overlaid on the base color
    /// </summary>
    /// <param name="color">The base color</param>
    /// <returns>The safe color for overlaying</returns>
    public static Color GetContrastedColor(this Color color) {
        var lightness = color.PerceivedLightness();
        // if (lightness > 0.6) {
        //     var ratio = 0.1f / Math.Max(color.r, Math.Max(color.g, color.b));
        //     return color * ratio;
        // } else if (lightness > 0.5) {
        //     return Color.black;
        // } else if (lightness > 0.4) {
        //     return Color.white;
        // } else {
        //     var ratio = 0.9f / Math.Min(color.r, Math.Min(color.g, color.b));
        //     return color * ratio;
        // }
        return lightness > 0.5 ? Color.black : Color.white;
    }

    /// <summary>
    /// Blends two colors together using an optional weight ratio.
    /// </summary>
    /// <param name="color">The base color.</param>
    /// <param name="other">The target color to mix in.</param>
    /// <param name="weight">
    /// The blend weight between 0.0 and 1.0.
    /// <c>0.0</c> returns pure base color, <c>0.5</c> returns an equal 50/50 mix, and <c>1.0</c> returns pure target color.
    /// </param>
    /// <returns>A new <see cref="Color"/> linearly interpolated between the two inputs.</returns>
    public static Color Mix(this Color color, Color other, float weight = 0.5f) {
        return Color.Lerp(color, other, weight);
    }

    /// <summary>
    /// Converts a hexadecimal uint representation (0xRRGGBB or 0xRRGGBBAA) to a <see cref="Color32"/>.
    /// </summary>
    /// <param name="hex">The color value in hex format.</param>
    /// <returns>A <see cref="Color32"/> corresponding to the hex input.</returns>
    public static Color32 ToColor32(this uint hex) {
        if (hex <= 0xFFFFFF) {
            return new Color32(
                (byte)((hex >> 16) & 0xFF),
                (byte)((hex >> 8) & 0xFF),
                (byte)(hex & 0xFF),
                255
            );
        }

        return new Color32(
            (byte)((hex >> 24) & 0xFF),
            (byte)((hex >> 16) & 0xFF),
            (byte)((hex >> 8) & 0xFF),
            (byte)(hex & 0xFF)
        );
    }

    /// <summary>
    /// Converts a signed hexadecimal int representation (0xRRGGBB or 0xRRGGBBAA) to a <see cref="Color32"/>.
    /// </summary>
    /// <param name="hex">The color value in hex format as an integer.</param>
    /// <returns>A <see cref="Color32"/> corresponding to the hex input.</returns>
    public static Color32 ToColor32(this int hex) {
        return unchecked((uint)hex).ToColor32();
    }
}
