using System.Linq.Expressions;
using System.Reflection;

namespace SimpleGL.Util.Extensions;
internal static class ReflectionExtensions {
    internal static bool IsReferenceType(this Type type) => !type.IsValueType && type != typeof(string);

    public static PropertyInfo GetProperty<T>(this Expression<Func<T>> property) {
        PropertyInfo propertyInfo = null;
        Expression body = property.Body;

        if (body is MemberExpression) {
            propertyInfo = (body as MemberExpression).Member as PropertyInfo;
        } else if (body is UnaryExpression) {
            propertyInfo = ((MemberExpression)((UnaryExpression)body).Operand).Member as PropertyInfo;
        }

        if (propertyInfo == null) {
            throw new ArgumentException("The lambda expression 'property' should point to a valid Property");
        }

        return propertyInfo;
    }

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

    public static Func<T> BuildGetAccessor<T>(Expression<Func<T>> propertySelector) {
        return propertySelector.GetProperty().GetGetMethod().CreateDelegate<Func<T>>();
    }

    public static Action<T> BuildSetAccessor<T>(Expression<Func<T>> propertySelector) {
        return propertySelector.GetProperty().GetSetMethod().CreateDelegate<Action<T>>();
    }

    public static bool HasAttribute(this Type type, Type attributeType) {
        return type.GetCustomAttributes(attributeType, false).Length > 0;
    }

    // a generic extension for CreateDelegate
    public static T CreateDelegate<T>(this MethodInfo method) where T : class {
        return Delegate.CreateDelegate(typeof(T), method) as T;
    }
}