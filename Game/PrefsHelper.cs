using System;
using System.Collections.Generic;

namespace NukeLib.Game;

/// <summary>
/// Helper to make subscribing to preference changes easy
/// </summary>
public static class PrefsHelper {
    private static readonly Dictionary<string, Action<object?>> _listeners = new();

    private static readonly Dictionary<(string key, Delegate callback), Action<object?>>
        _wrappers = new(); // For finding wrappers

    /// <summary>
    /// Subscribe to a certain preference change
    /// </summary>
    /// <param name="key">The key of the preference</param>
    /// <param name="callback">What to call when the preference changes</param>
    /// <typeparam name="T">The type of the preference. Note: the base game only handles int, float, and string.</typeparam>
    public static void Subscribe<T>(string key, Action<T> callback) {
        SubToPrefsManagerIfNecessary();

        Action<object?> wrapper = obj => {
            if (obj is T typedValue) {
                callback(typedValue);
            } else if (obj == null && default(T) == null) {
                callback(default!);
            }
        };

        // Store using the base Delegate reference
        _wrappers[(key, callback)] = wrapper;

        if (!_listeners.ContainsKey(key)) {
            _listeners[key] = _ => { };
        }

        _listeners[key] += wrapper;
    }

    /// <summary>
    /// Unsubscribe to a preference change.
    /// </summary>
    /// <param name="key">The key of the preference</param>
    /// <param name="callback">What to call when the preference changes</param>
    /// <typeparam name="T">The type of the preference. Note: the base game only handles int, float, and string.</typeparam>
    public static void Unsubscribe<T>(string key, Action<T> callback) {
        if (_wrappers.TryGetValue((key, callback), out var wrapper)) {
            if (_listeners.ContainsKey(key)) {
                _listeners[key] -= wrapper;
            }

            _wrappers.Remove((key, callback));
        }
    }

    private static bool _slappedTheBell = false;

    private static void SubToPrefsManagerIfNecessary() {
        if (_slappedTheBell) return;
        _slappedTheBell = true;
        PrefsManager.onPrefChanged += OnPrefChanged;
    }

    private static void OnPrefChanged(string key, object? obj) {
        if (_listeners.TryGetValue(key, out var callback)) {
            callback?.Invoke(obj);
        }
    }
}
