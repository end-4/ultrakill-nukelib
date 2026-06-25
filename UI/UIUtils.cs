using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NukeLib.UI;

public static class UIUtils {
    /// <summary>
    /// Pretty much GameObject.Find but also considers inactive objects and works for nested items
    /// </summary>
    /// <param name="baseObject">The base object for the path</param>
    /// <param name="path">The object path going from the base object</param>
    /// <returns></returns>
    public static GameObject FindRecursive(GameObject baseObject, string path) {
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
    /// <returns></returns>
    public static GameObject FindRecursive(string path) {
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
            Plugin.Log.LogWarning($"Root item {firstItem} not found for object path {baseObject.name}/{path}");
            return null;
        }
        return FindRecursive(baseObject, restPath);
    }
}
