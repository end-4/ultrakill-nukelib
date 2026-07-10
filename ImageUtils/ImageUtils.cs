using System;
using System.Collections.Generic;
using TagLib;
using System.IO;
using UnityEngine;
using File = System.IO.File;
using Object = UnityEngine.Object;

namespace NukeLib.ImageUtils;

/// <summary>
/// Helper for some image processing stuff
/// </summary>
public static class ImageUtils {
    private static readonly string[] SupportedImageExtensions = [".png", ".jpg", ".jpeg"];
    private static readonly string[] SupportedAudioExtensions = [".mp3", ".ogg"]; // .wav doesn't have images

    /// <summary>
    /// Extract the cover of an audio file
    /// </summary>
    /// <param name="audioFilePath">The file path</param>
    /// <returns>Texture2D of the cover if there is one, otherwise null</returns>
    private static Texture2D? ExtractAudioCover(string audioFilePath) {
        try {
            using var file = TagLib.File.Create(audioFilePath);
            // Check if any pictures are embedded in the tags
            if (file.Tag.Pictures.Length > 0) {
                var pic = file.Tag.Pictures[0];
                byte[] imgBytes = pic.Data.Data;

                Texture2D tex = new Texture2D(2, 2);
                if (tex.LoadImage(imgBytes)) {
                    return tex;
                }

                Object.Destroy(tex);
            }
        } catch (Exception e) {
            return null;
        }

        return null;
    }

    /// <summary>
    /// Gets the dominant color of a file
    /// </summary>
    /// <param name="sourceTexture">The image to grab dominant color from</param>
    /// <returns>The dominant color</returns>
    public static Color GetDominantColor(Texture2D sourceTexture) {
        RenderTexture rt = RenderTexture.GetTemporary(1, 1, 0, RenderTextureFormat.ARGB32);

        // Copy source texture into the 1x1 texture
        Graphics.Blit(sourceTexture, rt);

        // Read the single pixel
        RenderTexture.active = rt;
        Texture2D tex1x1 = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex1x1.ReadPixels(new Rect(0, 0, 1, 1), 0, 0);
        tex1x1.Apply();

        Color dominantColor = tex1x1.GetPixel(0, 0);

        // Clean up
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        Object.Destroy(tex1x1);

        return dominantColor;
    }

    /// <summary>
    /// Gets the dominant color of an image or audio file with cover
    /// </summary>
    /// <param name="filePath">Path to file</param>
    /// <returns>The dominant color</returns>
    public static Color GetDominantColor(string filePath) {
        if (!File.Exists(filePath)) {
            return Color.black;
        }

        var ext = Path.GetExtension(filePath).ToLower();
        Texture2D? tempTexture;
        if (Array.IndexOf(SupportedImageExtensions, ext) != -1) {
            byte[] fileData = File.ReadAllBytes(filePath);
            tempTexture = new Texture2D(2, 2);

            if (!tempTexture.LoadImage(fileData)) {
                Object.Destroy(tempTexture);
                return Color.black;
            }
        } else if (Array.IndexOf(SupportedAudioExtensions, ext) != -1) {
            tempTexture = ExtractAudioCover(filePath);
            if (tempTexture == null) return Color.black;
        } else {
            return Color.black;
        }

        var result = GetDominantColor(tempTexture);
        Object.Destroy(tempTexture);
        return result;
    }

    /// <summary>
    /// Scans a directory for files and returns the path of the file whose dominant color is closest to the target color.
    /// </summary>
    /// <param name="targetColor">The color to match</param>
    /// <param name="directoryPath">The directory containing the files</param>
    /// <param name="recursiveSearch">Whether to consider files in subdirectories</param>
    /// <param name="colorGrabFunction">The function to grab color from file. Defaults to one that grabs from an image. If there is an error, it must return Color.black</param>
    /// <returns>The path of the closest image file, or string.Empty if none found</returns>
    public static string FindClosestColorFile(Color targetColor, string directoryPath, bool recursiveSearch = false,
        Func<string, Color>? colorGrabFunction = null) {
        if (!Directory.Exists(directoryPath)) {
            return string.Empty;
        }

        colorGrabFunction ??= GetDominantColor;
        string[] files = Directory.GetFiles(directoryPath, "*.*",
            recursiveSearch ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        string closestFilePath = string.Empty;
        float minDistance = float.MaxValue;

        foreach (string file in files) {
            Color dominantColor = colorGrabFunction(file);
            if (dominantColor == Color.black) continue;
            // Distance
            float deltaR = targetColor.r - dominantColor.r;
            float deltaG = targetColor.g - dominantColor.g;
            float deltaB = targetColor.b - dominantColor.b;
            float distance = (deltaR * deltaR) + (deltaG * deltaG) + (deltaB * deltaB);
            // Update
            if (distance < minDistance) {
                minDistance = distance;
                closestFilePath = file;
            }
        }

        return closestFilePath;
    }

    /// <summary>
    /// Scans a directory for images and returns the path of the file whose dominant color is closest to the target color.
    /// </summary>
    /// <param name="targetColor">The color to match</param>
    /// <param name="directoryPath">The directory containing the images</param>
    /// <returns>The path of the closest image file, or string.Empty if none found</returns>
    public static string FindClosestColorImage(Color targetColor, string directoryPath) {
        return FindClosestColorFile(targetColor, directoryPath, recursiveSearch: false);
    }
}
