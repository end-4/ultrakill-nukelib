using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NukeLib.UI;

public static class UIUtils {
    /// <summary>
    /// Pretty much GameObject.Find but also considers inactive objects and works for nested items
    /// </summary>
    /// <param name="baseObject">The base object for the path</param>
    /// <param name="path">The object path going from the base object</param>
    /// <returns></returns>
    public static GameObject? FindRecursive(this GameObject baseObject, string path) {
        Transform t = baseObject.transform;
        string[] pathItems = path.Split("/");
        for (int i = 0; i < pathItems.Length; i++) {
            string itemStr = pathItems[i];
            t = t.transform.Find(itemStr);
            if (t == null) {
                Plugin.Log.LogWarning($"{itemStr} not found for object path {baseObject.name}/{path}");
                return null;
            }
        }

        return t.gameObject;
    }

    /// <summary>
    /// Pretty much GameObject.Find but also considers inactive objects and works for nested items
    /// </summary>
    /// <param name="path">The object path</param>
    /// <param name="warnings">Whether to warn when the target is not found. Set to false when you know it might be not found.</param>
    /// <returns></returns>
    public static GameObject? FindRecursive(string path) {
        int slashIndex = path.IndexOf('/');
        string firstItem = "";
        string restPath = "";
        if (slashIndex != -1) {
            firstItem = path.Substring(0, slashIndex);
            restPath = path.Substring(slashIndex + 1);
        }

        GameObject baseObject = SceneManager.GetActiveScene().GetRootGameObjects()
            .Where(obj => obj.name == firstItem).FirstOrDefault();
        if (baseObject == null) {
            Plugin.Log.LogWarning($"Root item not found for object path {path}");
            return null;
        }

        return FindRecursive(baseObject, restPath);
    }

    /// <summary>
    /// Forces layouts to update
    /// </summary>
    /// <param name="uiObject">The GameObject to update</param>
    public static void UnfuckLayoutHack(this GameObject uiObject) {
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)uiObject.transform);
    }

    /// <summary>
    /// Sets the layer for a GameObject and its descendants
    /// </summary>
    /// <param name="obj">The target GameObject</param>
    /// <param name="newLayer">The layer to set</param>
    public static void SetLayerRecursive(this GameObject obj, int newLayer) {
        if (obj == null) return;
        obj.layer = newLayer;

        foreach (Transform child in obj.transform) {
            SetLayerRecursive(child.gameObject, newLayer);
        }
    }

    /// <summary>
    /// Sets the specified material on all Image components attached to this GameObject and its descendants.
    /// </summary>
    /// <param name="obj">The target GameObject</param>
    /// <param name="material">The material to assign</param>
    public static void SetMaterialRecursive(this GameObject obj, Material material) {
        if (obj == null) return;
        Image[] images = obj.GetComponentsInChildren<Image>(includeInactive: true);
        foreach (Image img in images) {
            img.material = material;
        }
    }

    // NEW WARNING PARAM VARIANT AS OVERLOAD TO AVOID BREAKAGE

    /// <summary>
    /// Pretty much GameObject.Find but also considers inactive objects and works for nested items
    /// </summary>
    /// <param name="baseObject">The base object for the path</param>
    /// <param name="path">The object path going from the base object</param>
    /// <param name="warnings">Whether to warn when the target is not found. Set to false when you know it might be not found.</param>
    /// <returns></returns>
    public static GameObject? FindRecursive(this GameObject baseObject, string path, bool warnings = true) {
        Transform t = baseObject.transform;
        string[] pathItems = path.Split("/");
        for (int i = 0; i < pathItems.Length; i++) {
            string itemStr = pathItems[i];
            t = t.transform.Find(itemStr);
            if (t == null) {
                if (warnings) Plugin.Log.LogWarning($"{itemStr} not found for object path {baseObject.name}/{path}");
                return null;
            }
        }

        return t.gameObject;
    }

    /// <summary>
    /// Pretty much GameObject.Find but also considers inactive objects and works for nested items
    /// </summary>
    /// <param name="path">The object path</param>
    /// <param name="warnings">Whether to warn when the target is not found. Set to false when you know it might be not found.</param>
    /// <returns></returns>
    public static GameObject? FindRecursive(string path, bool warnings = true) {
        int slashIndex = path.IndexOf('/');
        string firstItem = "";
        string restPath = "";
        if (slashIndex != -1) {
            firstItem = path.Substring(0, slashIndex);
            restPath = path.Substring(slashIndex + 1);
        }

        GameObject baseObject = SceneManager.GetActiveScene().GetRootGameObjects()
            .Where(obj => obj.name == firstItem).FirstOrDefault();
        if (baseObject == null) {
            Plugin.Log.LogWarning($"Root item not found for object path {path}");
            return null;
        }

        return FindRecursive(baseObject, restPath, warnings: warnings);
    }
}
