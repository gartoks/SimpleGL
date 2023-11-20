using System.Reflection;

namespace SimpleGL.Util;
internal static class ReflectionHelper {

    public static void SetProperty<T>(object obj, string propertyName, T value) {
        if (obj == null) {
            throw new ArgumentNullException(nameof(obj));
        }

        Type type = obj.GetType();
        PropertyInfo? propertyInfo = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // If property exists and has a setter
        if (propertyInfo != null && propertyInfo.CanWrite) {
            propertyInfo.SetValue(obj, value);
            return;
        }

        // If no setter, try setting the backing field directly
        FieldInfo? fieldInfo = type.GetField($"<{propertyName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        if (fieldInfo != null) {
            fieldInfo.SetValue(obj, value);
            return;
        }

        throw new ArgumentException($"Property '{propertyName}' not found or it doesn't have a setter or a recognizable backing field.", nameof(propertyName));
    }

    public static T GetProperty<T>(object obj, string propertyName) {
        if (obj == null) {
            throw new ArgumentNullException(nameof(obj));
        }

        Type type = obj.GetType();
        PropertyInfo? propertyInfo = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (propertyInfo == null) {
            throw new ArgumentException($"Property '{propertyName}' not found in type '{type}'.", nameof(propertyName));
        }

        return (T)propertyInfo.GetValue(obj);
    }
}
