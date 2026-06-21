using System;
using System.Reflection;

namespace NukeLib.Reflection;

public static class ReflectionUtils {
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
        var bindingAttr = BindingFlags.NonPublic | BindingFlags.Instance;
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
        var bindingAttr = BindingFlags.NonPublic | BindingFlags.Instance;
        var privateField = t.GetField(fieldName, bindingAttr);

        if (privateField == null) throw new ArgumentException($"Field '{fieldName}' doesn't exist in class {t.Name}");

        privateField.SetValue(obj, value);
    }
}
