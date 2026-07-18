using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace NukeLib.Utils;

public static class FileAssetUtils {
    /// <summary>
    /// Creates a sprite from an image file
    /// </summary>
    /// <param name="filePath">Path to image file</param>
    /// <param name="pixelsPerUnit"></param>
    /// <param name="spriteType"></param>
    /// <returns>The new sprite</returns>
    public static Sprite LoadNewSprite(string filePath, float pixelsPerUnit = 100.0f,
        SpriteMeshType spriteType = SpriteMeshType.Tight) {
        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference

        Texture2D spriteTex = LoadTexture(filePath);
        Sprite newSprite = Sprite.Create(spriteTex, new Rect(0, 0, spriteTex.width, spriteTex.height),
            new Vector2(0, 0), pixelsPerUnit, 0, spriteType);

        return newSprite;
    }

    /// <summary>
    /// Constructs a sprite for a given texture.
    /// </summary>
    /// <param name="texture">The texture</param>
    /// <param name="pixelsPerUnit"></param>
    /// <param name="spriteType"></param>
    /// <returns>The new sprite</returns>
    public static Sprite ConvertTextureToSprite(Texture2D texture, float pixelsPerUnit = 100.0f,
        SpriteMeshType spriteType = SpriteMeshType.Tight) {
        // Converts a Texture2D to a sprite, assign this texture to a new sprite and return its reference

        Sprite NewSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0),
            pixelsPerUnit, 0, spriteType);

        return NewSprite;
    }

    /// <summary>
    /// Creates a Texture2D from an image file
    /// </summary>
    /// <param name="FilePath">Path to image file</param>
    /// <returns></returns>
    public static Texture2D LoadTexture(string FilePath) {
        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails

        if (File.Exists(FilePath)) {
            byte[] fileData;
            Texture2D tex;
            fileData = File.ReadAllBytes(FilePath);
            tex = new Texture2D(2, 2); // Create new "empty" texture
            if (tex.LoadImage(fileData)) // Load the imagedata into the texture (size is set automatically)
                return tex; // If data = readable -> return texture
        }

        return null; // Return null if load failed
    }

    /// <summary>
    /// Creates a Texture2D from an image file, asynchronously
    /// </summary>
    /// <param name="filePath">Path to image file</param>
    /// <returns>the texture</returns>
    public static async Task<Texture2D> LoadTextureAsync(string filePath) {
        if (!File.Exists(filePath)) return null;

        string url = "file://" + filePath;
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url)) {
            var operation = uwr.SendWebRequest();

            while (!operation.isDone) {
                await Task.Yield();
            }

            if (uwr.result != UnityWebRequest.Result.Success) {
                return null;
            }

            return DownloadHandlerTexture.GetContent(uwr);
        }
    }
}
