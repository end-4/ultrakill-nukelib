using System;
using System.Collections.Generic;
using System.Reflection;

namespace NukeLib.Utils;

/// <summary>
/// Class containing reflection stuff
/// </summary>
public static class ReflectionUtils {
    private const BindingFlags AnyInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    private static readonly Dictionary<(Type, string), MethodInfo> MethodCache = [];

    /// <summary>
    /// Gets a private field
    /// </summary>
    /// <param name="obj">The target object</param>
    /// <param name="fieldName">The field's name</param>
    /// <typeparam name="T">Type of The field to get</typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentException">When The field doesn't exist</exception>
    public static T GetPrivate<T>(this object obj, string fieldName) {
        Type t = obj.GetType();
        var bindingAttr = AnyInstance;
        var privateField = t.GetField(fieldName, bindingAttr);

        if (privateField == null) throw new ArgumentException($"Field '{fieldName}' doesn't exist in class {t.Name}");

        return (T)privateField.GetValue(obj);
    }

    /// <summary>
    /// Sets a private field
    /// </summary>
    /// <param name="obj">The target object</param>
    /// <param name="fieldName">The field's name</param>
    /// <param name="value">The value to set</param>
    /// <typeparam name="T">The type of the field</typeparam>
    /// <exception cref="ArgumentException">When the field doesn't exist</exception>
    public static void SetPrivate<T>(this object obj, string fieldName, T value) {
        Type t = obj.GetType();
        var bindingAttr = AnyInstance;
        var privateField = t.GetField(fieldName, bindingAttr);

        if (privateField == null) throw new ArgumentException($"Field '{fieldName}' doesn't exist in class {t.Name}");

        privateField.SetValue(obj, value);
    }

    /// <summary>
    /// Invokes a private instance method and returns its value.
    /// </summary>
    /// <param name="obj">The target object</param>
    /// <param name="methodName">The method's name</param>
    /// <param name="parameters">The parameters to pass to the method</param>
    /// <typeparam name="T">The expected return type of the method</typeparam>
    /// <returns>The return value of the invoked method</returns>
    /// <exception cref="ArgumentException">When the method doesn't exist</exception>
    public static T InvokePrivate<T>(this object obj, string methodName, params object[] parameters) {
        Type t = obj.GetType();
        var bindingAttr = AnyInstance;
        var privateMethod = t.GetMethod(methodName, bindingAttr);

        if (privateMethod == null)
            throw new ArgumentException($"Method '{methodName}' doesn't exist in class {t.Name}");

        return (T)privateMethod.Invoke(obj, parameters);
    }

    /// <summary>
    /// Invokes a private instance method that returns void
    /// </summary>
    /// <param name="obj">The target object</param>
    /// <param name="methodName">The method's name</param>
    /// <param name="parameters">The parameters to pass to the method</param>
    /// <exception cref="ArgumentException">When the method doesn't exist</exception>
    public static void InvokePrivate(this object obj, string methodName, params object[] parameters) {
        Type t = obj.GetType();
        var bindingAttr = AnyInstance;
        var privateMethod = t.GetMethod(methodName, bindingAttr);

        if (privateMethod == null)
            throw new ArgumentException($"Method '{methodName}' doesn't exist in class {t.Name}");

        privateMethod.Invoke(obj, parameters);
    }

    /// <summary>
    /// Attempts to invoke a parameterless method on an object.
    /// </summary>
    /// <param name="obj">The object</param>
    /// <param name="methodName">The method name</param>
    /// <returns>True if method exists and invoked successfully, false otherwise</returns>
    public static bool TryInvoke(this object obj, string methodName) {
        if (obj == null) return false;

        Type type = obj.GetType();
        MethodInfo method = null;

        while (type != null && method == null) {
            method = type.GetMethod(methodName, AnyInstance, null, Type.EmptyTypes, null);
            type = type.BaseType;
        }

        if (method == null) return false;

        try {
            method.Invoke(obj, null);
            return true;
        } catch {
            return false;
        }
    }

    /// <summary>
    /// Attempts to invoke the first matching parameterless method of an object from an array of target names.
    /// </summary>
    /// <param name="obj">The object</param>
    /// <param name="methodNames">The array containing possible method names</param>
    /// <returns>True if method exists and invoked successfully, false otherwise</returns>
    public static bool TryInvokeAny(this object obj, params string[] methodNames) {
        if (obj == null) return false;
        Type type = obj.GetType();

        foreach (string name in methodNames) {
            var key = (type, name);

            if (!MethodCache.TryGetValue(key, out MethodInfo method)) {
                Type currentType = type;
                while (currentType != null && method == null) {
                    method = currentType.GetMethod(name, AnyInstance, null, Type.EmptyTypes, null);
                    currentType = currentType.BaseType;
                }

                MethodCache[key] = method;
            }

            if (method != null) {
                try {
                    method.Invoke(obj, null);
                    return true;
                } catch {
                    return false;
                }
            }
        }

        return false;
    }
}
