using System.Linq.Expressions;

namespace AstQuerying.Queries.Registry.Implementations;

internal static class ExpressionNormalizer
{
    public static Expression Coerce(Expression value, Type targetType)
    {
        if (value.Type == targetType)
            return value;

        var underlyingTarget = Nullable.GetUnderlyingType(targetType) ?? targetType;
        var underlyingValue = Nullable.GetUnderlyingType(value.Type) ?? value.Type;

        if (targetType.IsAssignableFrom(value.Type))
            return Expression.Convert(value, targetType);

        if (underlyingTarget == underlyingValue)
            return Expression.Convert(value, targetType);

        return Expression.Convert(Expression.Convert(value, underlyingTarget), targetType);
    }
}
