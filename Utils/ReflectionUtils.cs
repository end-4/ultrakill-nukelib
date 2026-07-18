using System;
using System.Reflection;

namespace NukeLib.Utils;

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

    /// <summary>
    /// Invokes a private instance method and returns its value.
    /// </summary>
    /// <param name="obj">The target object</param>
    /// <param name="methodName">The method's name</param>
    /// <param name="parameters">The parameters to pass to the method</param>
    /// <typeparam name="T">The expected return type of the method</typeparam>
    /// <returns>The return value of the invoked method</returns>
    /// <exception cref="ArgumentException">When the method doesn't exist</exception>
    public static T InvokePrivate<T>(this object obj, string methodName, params object[] parameters)
    {
        Type t = obj.GetType();
        var bindingAttr = BindingFlags.NonPublic | BindingFlags.Instance;
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
    public static void InvokePrivate(this object obj, string methodName, params object[] parameters)
    {
        Type t = obj.GetType();
        var bindingAttr = BindingFlags.NonPublic | BindingFlags.Instance;
        var privateMethod = t.GetMethod(methodName, bindingAttr);

        if (privateMethod == null)
            throw new ArgumentException($"Method '{methodName}' doesn't exist in class {t.Name}");

        privateMethod.Invoke(obj, parameters);
    }
}
