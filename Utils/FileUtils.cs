using System.Collections.Generic;
using System.IO;

namespace NukeLib.Utils;

/// <summary>
/// Utilities for file stuff
/// </summary>
public static class FileUtils {
    /// <summary>
    /// Gets the items of a directory.
    /// For example /home/end -> [/, /home, /home/end]
    /// </summary>
    /// <param name="directory">The DirectoryInfo</param>
    /// <returns>Items on the path</returns>
    public static IReadOnlyList<DirectoryInfo> GetPathItems(this DirectoryInfo directory) {
        if (directory == null) return [];

        var items = new List<DirectoryInfo>();
        var current = directory;

        while (current != null) {
            items.Add(current);
            current = current.Parent;
        }

        items.Reverse();
        return items;
    }
}
