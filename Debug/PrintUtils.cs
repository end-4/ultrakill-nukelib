using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace NukeLib.Debug;

/// <summary>
/// Class for convenient printing
/// </summary>
public static class PrintUtils {
    /// <summary>
    /// Turns array into a string of all elements
    /// </summary>
    /// <param name="array">The array</param>
    /// <param name="delimiter">The string to put between values</param>
    /// <param name="prefix">The string to put in front of the string</param>
    /// <param name="suffix">The string to put at the back of the string</param>
    /// <typeparam name="T">Type of the array</typeparam>
    /// <returns>The string representation of all array elements</returns>
    public static string Stringify<T>(this T[] array, string delimiter = ", ", string prefix = "[", string suffix = "]") {
        string result = prefix;
        for (int i = 0; i < array.Length; i++) {
            result += array[i].ToString();
            if (i < array.Length - 1) result += delimiter;
        }

        result += suffix;
        return result;
    }
}
