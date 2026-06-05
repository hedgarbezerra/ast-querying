using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Functions.Contracts;

namespace AstQuerying.Queries.Functions.Implementations;

/// <summary>
/// Built-in <see cref="IFunctionProvider"/> for the current UTC calendar date (date component only).
/// </summary>
public sealed class TodayFunction : IFunctionProvider
{
    /// <inheritdoc />
    public string Name => "Today";

    /// <inheritdoc />
    public string DisplayName => "Today";

    /// <inheritdoc />
    public string Description => "Current date (date component only).";

    /// <inheritdoc />
    public IReadOnlyList<Type> SupportedTypes { get; } = new[] { typeof(DateTime), typeof(DateOnly) };

    /// <inheritdoc />
    public object? GetValue(Type targetType)
    {
        if (targetType == typeof(DateOnly))
            return DateOnly.FromDateTime(DateTime.Today);

        if (targetType == typeof(DateTime))
            return DateTime.Today;

        throw new QueryConversionException($"Today is not supported for type {targetType.Name}.");
    }
}

/// <summary>
/// Built-in provider for the current UTC date and time.
/// </summary>
public sealed class NowFunction : IFunctionProvider
{
    /// <inheritdoc />
    public string Name => "Now";

    /// <inheritdoc />
    public string DisplayName => "Now";

    /// <inheritdoc />
    public string Description => "Current UTC date and time.";

    /// <inheritdoc />
    public IReadOnlyList<Type> SupportedTypes { get; } = new[] { typeof(DateTime), typeof(DateTimeOffset) };

    /// <inheritdoc />
    public object? GetValue(Type targetType)
    {
        if (targetType == typeof(DateTime))
            return DateTime.UtcNow;

        if (targetType == typeof(DateTimeOffset))
            return DateTimeOffset.UtcNow;

        throw new QueryConversionException($"Now is not supported for type {targetType.Name}.");
    }
}

/// <summary>
/// Built-in provider for the first instant of the current UTC month.
/// </summary>
public sealed class StartOfMonthFunction : IFunctionProvider
{
    /// <inheritdoc />
    public string Name => "StartOfMonth";

    /// <inheritdoc />
    public string DisplayName => "Start of month";

    /// <inheritdoc />
    public string Description => "First day of the current month.";

    /// <inheritdoc />
    public IReadOnlyList<Type> SupportedTypes { get; } = new[] { typeof(DateTime), typeof(DateOnly) };

    /// <inheritdoc />
    public object? GetValue(Type targetType)
    {
        var today = DateTime.UtcNow;
        var start = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        if (targetType == typeof(DateTime))
            return start;

        if (targetType == typeof(DateOnly))
            return DateOnly.FromDateTime(start);

        throw new QueryConversionException($"StartOfMonth is not supported for type {targetType.Name}.");
    }
}

/// <summary>
/// Built-in provider for the last representable instant of the current UTC calendar month.
/// </summary>
public sealed class EndOfMonthFunction : IFunctionProvider
{
    /// <inheritdoc />
    public string Name => "EndOfMonth";

    /// <inheritdoc />
    public string DisplayName => "End of month";

    /// <inheritdoc />
    public string Description => "Last moment of the current calendar month.";

    /// <inheritdoc />
    public IReadOnlyList<Type> SupportedTypes { get; } = new[] { typeof(DateTime), typeof(DateOnly) };

    /// <inheritdoc />
    public object? GetValue(Type targetType)
    {
        var today = DateTime.UtcNow;
        var start = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddMonths(1).AddTicks(-1);

        if (targetType == typeof(DateTime))
            return end;

        if (targetType == typeof(DateOnly))
            return DateOnly.FromDateTime(end);

        throw new QueryConversionException($"EndOfMonth is not supported for type {targetType.Name}.");
    }
}

/// <summary>
/// Built-in provider for the current calendar year as an integer.
/// </summary>
public sealed class CurrentYearFunction : IFunctionProvider
{
    /// <inheritdoc />
    public string Name => "CurrentYear";

    /// <inheritdoc />
    public string DisplayName => "Current year";

    /// <inheritdoc />
    public string Description => "Current calendar year.";

    /// <inheritdoc />
    public IReadOnlyList<Type> SupportedTypes { get; } = new[] { typeof(int), typeof(short), typeof(long) };

    /// <inheritdoc />
    public object? GetValue(Type targetType)
    {
        var y = DateTime.UtcNow.Year;

        if (targetType == typeof(int))
            return y;

        if (targetType == typeof(short))
            return (short)y;

        if (targetType == typeof(long))
            return (long)y;

        throw new QueryConversionException($"CurrentYear is not supported for type {targetType.Name}.");
    }
}
