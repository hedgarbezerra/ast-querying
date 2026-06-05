using System.Globalization;
using System.Text.Json;
using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Filters.Contracts;

namespace AstQuerying.Queries.Filters.Implementations;

/// <summary>
/// Default <see cref="IValueConverter"/> supporting JSON primitives, common scalar types, enums, and nullable wrappers.
/// </summary>
public sealed class ValueConverter : IValueConverter
{
    /// <inheritdoc />
    public object? ConvertToType(object? rawValue, Type targetType, string? fieldPath)
    {
        try
        {
            return ConvertCore(rawValue, targetType);
        }
        catch (Exception ex) when (ex is not QueryConversionException)
        {
            throw new QueryConversionException(
                $"Cannot convert value for '{fieldPath ?? "?"}' to {targetType.Name}.", ex);
        }
    }

    private static object? ConvertCore(object? rawValue, Type targetType)
    {
        var nullableInner = Nullable.GetUnderlyingType(targetType);
        var effective = nullableInner ?? targetType;

        if (rawValue is null || rawValue is DBNull)
            return nullableInner is not null ? null : !targetType.IsValueType ? null : throw new QueryConversionException("Null not allowed for non-nullable value type.");

        if (nullableInner is not null && (rawValue is string s && string.IsNullOrWhiteSpace(s)))
            return null;

        if (rawValue is JsonElement je)
            rawValue = JsonElementToObject(je);

        if (rawValue is null)
            return nullableInner is not null ? null : !targetType.IsValueType ? null : throw new QueryConversionException("Null not allowed for non-nullable value type.");

        if (targetType.IsInstanceOfType(rawValue))
            return rawValue;

        if (effective == typeof(Guid) && rawValue is string gs)
            return Guid.Parse(gs);

        if (effective == typeof(byte[]) && rawValue is string bs)
            return Convert.FromBase64String(bs);

        if (effective == typeof(DateTime))
        {
            if (rawValue is DateTime dt)
                return dt;
            if (rawValue is DateTimeOffset dto)
                return dto.UtcDateTime;
            return DateTime.Parse(rawValue.ToString()!, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        }

        if (effective == typeof(DateOnly))
        {
            if (rawValue is DateOnly d0)
                return d0;
            return DateOnly.Parse(rawValue.ToString()!, CultureInfo.InvariantCulture);
        }

        if (effective == typeof(DateTimeOffset))
        {
            if (rawValue is DateTimeOffset d1)
                return d1;
            return DateTimeOffset.Parse(rawValue.ToString()!, CultureInfo.InvariantCulture);
        }

        if (effective.IsEnum)
            return Enum.Parse(effective, rawValue.ToString()!, true);

        return Convert.ChangeType(rawValue, effective, CultureInfo.InvariantCulture);
    }

    private static object? JsonElementToObject(JsonElement je)
    {
        return je.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String => je.GetString(),
            JsonValueKind.Number => je.TryGetInt64(out var l) ? l : je.GetDouble(),
            JsonValueKind.Array => je,
            JsonValueKind.Object => je,
            _ => je.ToString()
        };
    }
}
