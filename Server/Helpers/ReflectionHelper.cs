using System.Collections;
using System.Globalization;
using System.Reflection;
using SPTarkov.Server.Core.Models.Common;

namespace AmmoGen.Helpers;

public static class ReflectionHelper
{
    public static void SetPropertyOrField(object target, string name, object value)
    {
        var type = target.GetType();
        var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (prop != null && prop.CanWrite)
        {
            prop.SetValue(target, ConvertValue(value, prop.PropertyType));
            return;
        }

        var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (field != null)
            field.SetValue(target, ConvertValue(value, field.FieldType));
    }

    public static object? GetPropertyOrField(object target, string name)
    {
        var type = target.GetType();
        var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (prop != null)
            return prop.GetValue(target);

        var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        return field?.GetValue(target);
    }

    public static object? ConvertValue(object value, Type targetType)
    {
        if (value == null)
            return value;

        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        if (underlyingType == typeof(MongoId) && value is string str)
            return new MongoId(str);

        if (targetType.IsEnum && value is string enumStr)
            return Enum.Parse(targetType, enumStr, true);

        if (value.GetType() == underlyingType || value.GetType().IsAssignableTo(underlyingType))
            return value;

        if (value is IConvertible && typeof(IConvertible).IsAssignableFrom(underlyingType))
            return Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);

        return value;
    }

    public static bool AddToFilterList(object filterList, string id)
    {
        if (filterList == null)
            return false;

        var enumerable = filterList as IEnumerable ?? (filterList as IEnumerable<object>);
        if (enumerable == null)
            return false;

        var existing = new HashSet<string>(enumerable.Cast<object>().Select(o => o?.ToString() ?? string.Empty));
        if (existing.Contains(id))
            return false;

        var type = filterList.GetType();
        var elementType = type.IsGenericType
            ? type.GetGenericArguments()[0]
            : typeof(object);
        var value = elementType == typeof(MongoId) || elementType.IsAssignableFrom(typeof(MongoId))
            ? (object)new MongoId(id)
            : id;

        var addMethod = type.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance, null, new[] { elementType }, null);
        if (addMethod == null)
            return false;

        addMethod.Invoke(filterList, new[] { value });
        return true;
    }

    public static bool FilterListContains(object filterList, string id)
    {
        if (filterList == null)
            return false;

        var enumerable = filterList as IEnumerable ?? (filterList as IEnumerable<object>);
        if (enumerable == null)
            return false;

        return enumerable.Cast<object>().Any(o => (o?.ToString() ?? string.Empty).Equals(id, StringComparison.OrdinalIgnoreCase));
    }
}
