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

    /// <summary>
    /// Converts a hexadecimal uint representation (0xRRGGBB or 0xRRGGBBAA) to a <see cref="Color"/>.
    /// </summary>
    /// <param name="hex">The color value in hex format.</param>
    /// <returns>A <see cref="Color"/> corresponding to the hex input.</returns>
    public static Color ToColor(this uint hex) {
        return hex.ToColor32();
    }

    /// <summary>
    /// Converts a signed hexadecimal int representation (0xRRGGBB or 0xRRGGBBAA) to a <see cref="Color"/>.
    /// </summary>
    /// <param name="hex">The color value in hex format as an integer.</param>
    /// <returns>A <see cref="Color"/> corresponding to the hex input.</returns>
    public static Color ToColor(this int hex) {
        return hex.ToColor32();
    }

    #region OKLCH / OKLab / XYZ / sRGB Conversions
    // Ported from https://gist.github.com/dkaraush/65d19d61396f5f3cd8ba7d1b4b3c9432
    //   which is an all-in-one snippet based on https://github.com/color-js/color.js/blob/main/src/spaces/oklch.js
    //   and color.js is under MIT license

    private static readonly float[] M_OKLAB_TO_LMSG = [
        1f,  0.3963377773761749f,  0.2158037573099136f,
        1f, -0.1055613458156586f, -0.0638541728258133f,
        1f, -0.0894841775298119f, -1.2914855480194092f
    ];

    private static readonly float[] M_LMS_TO_XYZ = [
        +1.2268798758459243f, -0.5578149944602171f,  0.2813910456659647f,
        -0.0405757452148008f,  1.1122868032803170f, -0.0717110580655164f,
        -0.0763729366746601f, -0.4214933324022432f,  1.5869240198367816f
    ];

    private static readonly float[] M_XYZ_TO_LMS = [
        0.8190224379967030f, 0.3619062600528904f, -0.1288737815209879f,
        0.0329836539323885f, 0.9292868615863434f,  0.0361446663506424f,
        0.0481771893596242f, 0.2642395317527308f,  0.6335478284694309f
    ];

    private static readonly float[] M_LMSG_TO_OKLAB = [
        0.2104542683093140f,  0.7936177747023054f, -0.0040720430116193f,
        1.9779985324311684f, -2.4285922420485799f,  0.4505937096174110f,
        0.0259040424655478f,  0.7827717124575296f, -0.8086757549230774f
    ];

    private static readonly float[] M_XYZ_TO_RGB_LINEAR = [
        +3.2409699419045226f,  -1.537383177570094f,   -0.4986107602930034f,
        -0.9692436362808796f,   1.8759675015077202f,   0.04155505740717559f,
        +0.05563007969699366f, -0.20397695888897652f,  1.0569715142428786f
    ];

    private static readonly float[] M_RGB_LINEAR_TO_XYZ = [
        0.41239079926595934f, 0.357584339383878f,   0.1804807884018343f,
        0.21263900587151027f, 0.715168678767756f,   0.07219231536073371f,
        0.01933081871559182f, 0.11919477979462598f, 0.9505321522496607f
    ];

    /// <summary>
    /// Multiplies a 3x3 matrix (flattened row-major into a 9-element array) with a 3-element vector.
    /// </summary>
    /// <param name="matrix">The 3x3 matrix (length 9).</param>
    /// <param name="vector">The 3-element vector (length 3).</param>
    /// <returns>The resulting 3-element float array.</returns>
    public static float[] MultiplyMatrices(float[] matrix, float[] vector) {
        return [
            matrix[0] * vector[0] + matrix[1] * vector[1] + matrix[2] * vector[2],
            matrix[3] * vector[0] + matrix[4] * vector[1] + matrix[5] * vector[2],
            matrix[6] * vector[0] + matrix[7] * vector[1] + matrix[8] * vector[2]
        ];
    }

    /// <summary>
    /// Multiplies a 3x3 matrix (flattened row-major into a 9-element array) with a <see cref="Vector3"/>.
    /// </summary>
    /// <param name="matrix">The 3x3 matrix (length 9).</param>
    /// <param name="vector">The <see cref="Vector3"/> input vector.</param>
    /// <returns>The resulting transformed <see cref="Vector3"/>.</returns>
    public static Vector3 MultiplyMatrix3x3(float[] matrix, Vector3 vector) {
        return new Vector3(
            matrix[0] * vector.x + matrix[1] * vector.y + matrix[2] * vector.z,
            matrix[3] * vector.x + matrix[4] * vector.y + matrix[5] * vector.z,
            matrix[6] * vector.x + matrix[7] * vector.y + matrix[8] * vector.z
        );
    }

    /// <summary>
    /// Converts OKLCH coordinates (L, C, H) to OKLab coordinates (L, a, b).
    /// </summary>
    /// <param name="oklch">The OKLCH vector where x = Lightness [0, 1], y = Chroma [0, ~0.4], z = Hue [0, 1) (normalized) or <see cref="float.NaN"/>.</param>
    /// <returns>The OKLab vector (L, a, b).</returns>
    public static Vector3 OklchToOklab(Vector3 oklch) {
        float l = oklch.x;
        float c = oklch.y;
        float h = oklch.z;

        if (float.IsNaN(h) || c <= 0f) {
            return new Vector3(l, 0f, 0f);
        }

        float angle = h * 2f * Mathf.PI;
        return new Vector3(
            l,
            c * Mathf.Cos(angle),
            c * Mathf.Sin(angle)
        );
    }

    /// <summary>
    /// Converts OKLab coordinates (L, a, b) to OKLCH coordinates (L, C, H).
    /// </summary>
    /// <param name="oklab">The OKLab vector where x = L, y = a, z = b.</param>
    /// <returns>The OKLCH vector where x = Lightness [0, 1], y = Chroma, z = Hue [0, 1) (or <see cref="float.NaN"/> for achromatic/gray).</returns>
    public static Vector3 OklabToOklch(Vector3 oklab) {
        float l = oklab.x;
        float a = oklab.y;
        float b = oklab.z;

        float c = Mathf.Sqrt(a * a + b * b);
        float h;

        if (Mathf.Abs(a) < 0.0002f && Mathf.Abs(b) < 0.0002f) {
            h = float.NaN;
        } else {
            float angle = Mathf.Atan2(b, a);
            h = ((angle / (2f * Mathf.PI)) % 1f + 1f) % 1f;
        }

        return new Vector3(l, c, h);
    }

    /// <summary>
    /// Converts a single gamma sRGB color component to linear sRGB.
    /// Supports signed/extended color values without hard clipping.
    /// (it'll be used in the method that takes/returns a Vector3, read that and it'll make sense)
    /// </summary>
    /// <param name="c">The sRGB channel value.</param>
    /// <returns>The linear sRGB channel value.</returns>
    public static float RgbToSrgbLinear(float c) {
        float absC = Mathf.Abs(c);
        float val = absC <= 0.04045f
            ? absC / 12.92f
            : Mathf.Pow((absC + 0.055f) / 1.055f, 2.4f);
        return c < 0f ? -val : val;
    }

    /// <summary>
    /// Converts gamma sRGB coordinates to linear sRGB coordinates.
    /// </summary>
    /// <param name="rgb">The sRGB color vector.</param>
    /// <returns>The linear sRGB vector.</returns>
    public static Vector3 RgbToSrgbLinear(Vector3 rgb) {
        return new Vector3(
            RgbToSrgbLinear(rgb.x),
            RgbToSrgbLinear(rgb.y),
            RgbToSrgbLinear(rgb.z)
        );
    }

    /// <summary>
    /// Converts gamma <see cref="Color"/> to linear sRGB <see cref="Color"/>.
    /// </summary>
    /// <param name="rgb">The sRGB <see cref="Color"/> with components in range [0, 1].</param>
    /// <returns>The linear sRGB <see cref="Color"/>.</returns>
    public static Color RgbToSrgbLinear(this Color rgb) {
        return new Color(
            RgbToSrgbLinear(rgb.r),
            RgbToSrgbLinear(rgb.g),
            RgbToSrgbLinear(rgb.b),
            rgb.a
        );
    }

    /// <summary>
    /// Converts a single linear sRGB color component to gamma sRGB.
    /// Supports signed/extended color values without hard clipping.
    /// </summary>
    /// <param name="c">The linear sRGB channel value.</param>
    /// <returns>The gamma sRGB channel value.</returns>
    public static float SrgbLinearToRgb(float c) {
        float absC = Mathf.Abs(c);
        float val = absC > 0.0031308f
            ? 1.055f * Mathf.Pow(absC, 1f / 2.4f) - 0.055f
            : 12.92f * absC;
        return c < 0f ? -val : val;
    }

    /// <summary>
    /// Converts linear sRGB coordinates to gamma sRGB coordinates.
    /// </summary>
    /// <param name="rgbLinear">The linear sRGB color vector.</param>
    /// <returns>The gamma sRGB color vector.</returns>
    public static Vector3 SrgbLinearToRgb(Vector3 rgbLinear) {
        return new Vector3(
            SrgbLinearToRgb(rgbLinear.x),
            SrgbLinearToRgb(rgbLinear.y),
            SrgbLinearToRgb(rgbLinear.z)
        );
    }

    /// <summary>
    /// Converts linear sRGB <see cref="Color"/> to gamma sRGB <see cref="Color"/>.
    /// </summary>
    /// <param name="rgbLinear">The linear sRGB <see cref="Color"/>.</param>
    /// <returns>The gamma sRGB <see cref="Color"/> clamped to [0, 1].</returns>
    public static Color SrgbLinearToRgb(this Color rgbLinear) {
        return new Color(
            Mathf.Clamp01(SrgbLinearToRgb(rgbLinear.r)),
            Mathf.Clamp01(SrgbLinearToRgb(rgbLinear.g)),
            Mathf.Clamp01(SrgbLinearToRgb(rgbLinear.b)),
            rgbLinear.a
        );
    }

    /// <summary>
    /// Converts OKLab coordinates (L, a, b) to CIE XYZ coordinates.
    /// </summary>
    /// <param name="oklab">The OKLab color vector.</param>
    /// <returns>The XYZ color vector.</returns>
    public static Vector3 OklabToXyz(Vector3 oklab) {
        Vector3 lmsg = MultiplyMatrix3x3(M_OKLAB_TO_LMSG, oklab);
        Vector3 lms = new Vector3(
            lmsg.x * lmsg.x * lmsg.x,
            lmsg.y * lmsg.y * lmsg.y,
            lmsg.z * lmsg.z * lmsg.z
        );
        return MultiplyMatrix3x3(M_LMS_TO_XYZ, lms);
    }

    /// <summary>
    /// Converts CIE XYZ coordinates to OKLab coordinates (L, a, b).
    /// </summary>
    /// <param name="xyz">The XYZ color vector.</param>
    /// <returns>The OKLab color vector.</returns>
    public static Vector3 XyzToOklab(Vector3 xyz) {
        Vector3 lms = MultiplyMatrix3x3(M_XYZ_TO_LMS, xyz);
        Vector3 lmsg = new Vector3(
            MathF.Cbrt(lms.x),
            MathF.Cbrt(lms.y),
            MathF.Cbrt(lms.z)
        );
        return MultiplyMatrix3x3(M_LMSG_TO_OKLAB, lmsg);
    }

    /// <summary>
    /// Converts CIE XYZ coordinates to linear sRGB coordinates.
    /// </summary>
    /// <param name="xyz">The XYZ color vector.</param>
    /// <returns>The linear sRGB color vector.</returns>
    public static Vector3 XyzToRgbLinear(Vector3 xyz) {
        return MultiplyMatrix3x3(M_XYZ_TO_RGB_LINEAR, xyz);
    }

    /// <summary>
    /// Converts linear sRGB coordinates to CIE XYZ coordinates.
    /// </summary>
    /// <param name="rgbLinear">The linear sRGB color vector.</param>
    /// <returns>The XYZ color vector.</returns>
    public static Vector3 RgbLinearToXyz(Vector3 rgbLinear) {
        return MultiplyMatrix3x3(M_RGB_LINEAR_TO_XYZ, rgbLinear);
    }

    /// <summary>
    /// Checks whether a linear sRGB coordinate is within the sRGB gamut within an epsilon tolerance.
    /// </summary>
    private static bool IsInSrgbGamut(Vector3 rgbLinear, float epsilon = 0.0001f) {
        return rgbLinear.x >= -epsilon && rgbLinear.x <= 1f + epsilon &&
               rgbLinear.y >= -epsilon && rgbLinear.y <= 1f + epsilon &&
               rgbLinear.z >= -epsilon && rgbLinear.z <= 1f + epsilon;
    }

    /// <summary>
    /// Converts OKLCH coordinates (L, C, H in range [0, 1]) to an sRGB <see cref="Color"/>
    /// using constant-hue chroma reduction gamut mapping.
    /// </summary>
    /// <param name="oklch">The OKLCH vector where x = Lightness [0, 1], y = Chroma [0, ~0.4], z = Hue [0, 1).</param>
    /// <param name="alpha">Optional alpha transparency [0, 1]. Default is 1.</param>
    /// <returns>The resulting sRGB <see cref="Color"/>.</returns>
    public static Color OklchToRgb(Vector3 oklch, float alpha = 1f) {
        float l = oklch.x;
        float c = oklch.y;
        float h = oklch.z;

        // Boundary conditions for pure black / white or out-of-range lightness
        if (l >= 1f) return new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
        if (l <= 0f) return new Color(0f, 0f, 0f, Mathf.Clamp01(alpha));

        Vector3 rgbLinear = XyzToRgbLinear(OklabToXyz(OklchToOklab(new Vector3(l, c, h))));

        // If the color is already inside the sRGB gamut, convert directly
        if (IsInSrgbGamut(rgbLinear)) {
            Vector3 rgb = SrgbLinearToRgb(rgbLinear);
            return new Color(
                Mathf.Clamp01(rgb.x),
                Mathf.Clamp01(rgb.y),
                Mathf.Clamp01(rgb.z),
                Mathf.Clamp01(alpha)
            );
        }

        // Gamut mapping: binary search to find the maximum Chroma at constant L and H that fits in sRGB
        float low = 0f;
        float high = c;
        Vector3 bestRgbLinear = rgbLinear;

        // 16 iterations gives precision of original Chroma / 2^16 (< 0.00001)
        for (int i = 0; i < 16; i++) {
            float mid = (low + high) * 0.5f;
            Vector3 testLinear = XyzToRgbLinear(OklabToXyz(OklchToOklab(new Vector3(l, mid, h))));
            if (IsInSrgbGamut(testLinear)) {
                bestRgbLinear = testLinear;
                low = mid;
            } else {
                high = mid;
            }
        }

        Vector3 mappedRgb = SrgbLinearToRgb(bestRgbLinear);
        return new Color(
            Mathf.Clamp01(mappedRgb.x),
            Mathf.Clamp01(mappedRgb.y),
            Mathf.Clamp01(mappedRgb.z),
            Mathf.Clamp01(alpha)
        );
    }


    /// <summary>
    /// Converts OKLCH coordinates (L, C, H in range [0, 1]) to an sRGB <see cref="Color"/>.
    /// </summary>
    /// <param name="l">Lightness in range [0, 1].</param>
    /// <param name="c">Chroma (typically [0, ~0.4]).</param>
    /// <param name="h">Hue in range [0, 1) (or <see cref="float.NaN"/> for achromatic/gray).</param>
    /// <param name="alpha">Optional alpha transparency [0, 1]. Default is 1.</param>
    /// <returns>The resulting sRGB <see cref="Color"/> with channels clamped to [0, 1].</returns>
    public static Color OklchToRgb(float l, float c, float h, float alpha = 1f) {
        return OklchToRgb(new Vector3(l, c, h), alpha);
    }

    /// <summary>
    /// Converts an sRGB <see cref="Color"/> to OKLCH coordinates (L, C, H).
    /// </summary>
    /// <param name="color">The sRGB <see cref="Color"/>.</param>
    /// <returns>A <see cref="Vector3"/> where x = Lightness [0, 1], y = Chroma [0, ~0.33 in sRGB], z = Hue [0, 1) (or <see cref="float.NaN"/> for gray).</returns>
    public static Vector3 RgbToOklch(this Color color) {
        Vector3 rgb = new Vector3(color.r, color.g, color.b);
        return RgbToOklch(rgb);
    }

    /// <summary>
    /// Converts sRGB coordinates to OKLCH coordinates (L, C, H).
    /// </summary>
    /// <param name="rgb">The sRGB vector where x = R, y = G, z = B.</param>
    /// <returns>A <see cref="Vector3"/> where x = Lightness [0, 1], y = Chroma, z = Hue [0, 1) (or <see cref="float.NaN"/> for gray).</returns>
    public static Vector3 RgbToOklch(Vector3 rgb) {
        Vector3 rgbLinear = RgbToSrgbLinear(rgb);
        Vector3 xyz = RgbLinearToXyz(rgbLinear);
        Vector3 oklab = XyzToOklab(xyz);
        return OklabToOklch(oklab);
    }

    /// <summary>
    /// Converts sRGB components (r, g, b) to OKLCH coordinates (L, C, H).
    /// </summary>
    /// <param name="r">Red component in range [0, 1].</param>
    /// <param name="g">Green component in range [0, 1].</param>
    /// <param name="b">Blue component in range [0, 1].</param>
    /// <returns>A <see cref="Vector3"/> where x = Lightness [0, 1], y = Chroma, z = Hue [0, 1) (or <see cref="float.NaN"/> for gray).</returns>
    public static Vector3 RgbToOklch(float r, float g, float b) {
        return RgbToOklch(new Vector3(r, g, b));
    }

    /// <summary>
    /// Converts OKLCH color space values (normalized [0-1]) to a Unity RGB Color.
    /// </summary>
    /// <param name="L">Lightness in range [0, 1].</param>
    /// <param name="C">Chroma (typically [0, ~0.4]).</param>
    /// <param name="H">Hue in range [0, 1)</param>
    /// <param name="alpha">Alpha channel [0, 1]</param>
    public static Color OKLCHToRGB(float L, float C, float H, float alpha = 1f) {
        return OklchToRgb(new Vector3(L, C, H), alpha);
    }

    /// <summary>
    /// Converts Unity RGB Color to OKLCH values
    /// </summary>
    /// <param name="rgbColor">The RGB color</param>
    /// <param name="L">Lightness in range [0, 1].</param>
    /// <param name="C">Chroma (typically [0, ~0.4]).</param>
    /// <param name="H">Hue in range [0, 1)</param>
    public static void RGBToOKLCH(this Color rgbColor, out float L, out float C, out float H) {
        var oklch = RgbToOklch(rgbColor);
        L = oklch.x;
        C = oklch.y;
        H = oklch.z;
    }

    #endregion
}

