using AET.SteamAbstraction.Vdf.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace AET.SteamAbstraction.Vdf.Utilities;

// Taken from https://github.com/shravan2x/Gameloop.Vdf

internal static class Extensions
{
    public static T? Value<T>(this IEnumerable<VToken> value)
    {
        return value.Value<VToken, T>();
    }

    public static TValue? Value<T, TValue>(this IEnumerable<T> value) where T : VToken
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        if (value is not VToken token)
            throw new ArgumentException("Source value must be a JToken.");

        return token.Convert<VToken, TValue>();
    }

    internal static TValue? Convert<T, TValue>(this T? token) where T : VToken
    {
        if (token == null)
            return default;

        // don't want to cast JValue to its interfaces, want to get the internal value
        if (token is TValue && typeof(TValue) != typeof(IComparable) && typeof(TValue) != typeof(IFormattable))
            return (TValue)(object)token;

        if (token is not VValue value)
            throw new InvalidCastException($"Cannot cast {token.GetType()} to {typeof(T)}.");

        if (value.Value is TValue u)
            return u;

        var targetType = typeof(TValue);

        if (ReflectionUtils.IsNullableType(targetType))
        {
            if (value.Value == null)
                return default;

            targetType = Nullable.GetUnderlyingType(targetType);
        }

        if (TryConvertVdf<TValue>(value.Value!, out var resultObj))
            return resultObj;

        return (TValue)System.Convert.ChangeType(value.Value, targetType!, CultureInfo.InvariantCulture);
    }

    private static bool TryConvertVdf<T>(object value, out T? result)
    {
        result = default;

        // It won't be null at this point, so just handle the nullable type.
        if ((typeof(T) == typeof(bool) || Nullable.GetUnderlyingType(typeof(T)) == typeof(bool)) && value is string valueString)
        {
            switch (valueString)
            {
                case "1":
                    result = (T)(object)true;
                    return true;

                case "0":
                    result = (T)(object)false;
                    return true;
            }
        }

        return false;
    }
}