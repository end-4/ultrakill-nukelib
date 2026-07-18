using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NukeLib.Utils;

/// <summary>
/// Helper for some image processing stuff
/// </summary>
public static class ImageUtils {
    /// <summary>
    /// Supported file extensions
    /// </summary>
    private static readonly string[] SupportedExtensions = [".png", ".jpg", ".jpeg"];

    // Caching
    private static int _maxCacheSize = 1000;
    private static readonly Dictionary<string, LinkedListNode<CacheItem>> _cacheDict = new();
    private static readonly LinkedList<CacheItem> _lruList = [];

    private struct CacheItem {
        public string FilePath;
        public Color DominantColor;
    }

    /// <summary>
    /// Gets or sets the maximum number of items the dominant color cache can hold.
    /// Defaults to 1000. Set to 0 to disable caching.
    /// </summary>
    public static int MaxCacheSize {
        get => _maxCacheSize;
        set {
            _maxCacheSize = Mathf.Max(0, value);
            TrimCache();
        }
    }

    /// <summary>
    /// Clears the dominant color cache.
    /// </summary>
    public static void ClearCache() {
        _cacheDict.Clear();
        _lruList.Clear();
    }

    private static void TrimCache() {
        while (_cacheDict.Count > _maxCacheSize) {
            var lastNode = _lruList.Last;
            if (lastNode != null) {
                _cacheDict.Remove(lastNode.Value.FilePath);
                _lruList.RemoveLast();
            }
        }
    }

    private static bool TryGetFromCache(string filePath, out Color color) {
        if (_maxCacheSize > 0 && _cacheDict.TryGetValue(filePath, out var node)) {
            // Move accessed item to the front of the LRU list
            _lruList.Remove(node);
            _lruList.AddFirst(node);
            color = node.Value.DominantColor;
            return true;
        }

        color = Color.black;
        return false;
    }

    private static void AddToCache(string filePath, Color color) {
        if (_maxCacheSize <= 0) return;

        if (_cacheDict.TryGetValue(filePath, out var node)) {
            // If it already exists, update it and move to front
            _lruList.Remove(node);
            _lruList.AddFirst(node);
            node.Value = new CacheItem { FilePath = filePath, DominantColor = color };
        } else {
            // Add new item to the front
            var newNode = new LinkedListNode<CacheItem>(new CacheItem { FilePath = filePath, DominantColor = color });
            _lruList.AddFirst(newNode);
            _cacheDict[filePath] = newNode;
            TrimCache();
        }
    }

    /// <summary>
    /// Gets the dominant color of an image
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
    /// Gets the dominant color of an image
    /// </summary>
    /// <param name="filePath">Path to image file</param>
    /// <returns>The dominant color</returns>
    public static Color GetDominantColor(string filePath) {
        if (TryGetFromCache(filePath, out Color cachedColor)) {
            return cachedColor;
        }

        if (!File.Exists(filePath)) {
            return Color.black;
        }

        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D tempTexture = new Texture2D(2, 2);

        if (!tempTexture.LoadImage(fileData)) {
            Object.Destroy(tempTexture);
            return Color.black;
        }

        Color result = GetDominantColor(tempTexture);
        Object.Destroy(tempTexture);

        // Save the result to cache for future requests
        AddToCache(filePath, result);

        return result;
    }

    /// <summary>
    /// Scans a directory for images and returns the path of the file whose dominant color is closest to the target color.
    /// </summary>
    /// <param name="targetColor">The color to match</param>
    /// <param name="directoryPath">The directory containing the images</param>
    /// <returns>The path of the closest image file, or string.Empty if none found</returns>
    public static string FindClosestColorImage(Color targetColor, string directoryPath) {
        if (!Directory.Exists(directoryPath)) {
            return string.Empty;
        }

        string[] files = Directory.GetFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly);
        string closestFilePath = string.Empty;
        float minDistance = float.MaxValue;

        foreach (string file in files) {
            string ext = Path.GetExtension(file).ToLower();
            if (Array.IndexOf(SupportedExtensions, ext) == -1) continue;

            Color dominantColor = GetDominantColor(file);

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
}
