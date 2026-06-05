using System.Linq.Expressions;
using System.Reflection;
using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Metadata.ValueObjects;

namespace AstQuerying.Queries.Registry.Implementations;

internal static class MemberPathAnalyzer
{
    public static IReadOnlyList<(string Name, Type Type)> UnwrapMemberChain(Expression body)
    {
        var list = new List<(string, Type)>();
        Expression? cur = body;
        while (cur is not null)
        {
            switch (cur)
            {
                case MemberExpression m:
                    list.Insert(0, (m.Member.Name, m.Type));
                    cur = m.Expression;
                    break;
                case UnaryExpression { Operand: var op } u when u.NodeType == ExpressionType.Convert:
                    cur = op;
                    break;
                case ParameterExpression:
                    cur = null;
                    break;
                default:
                    throw new QueryConfigurationException($"Unsupported expression in member path: {cur.NodeType}.");
            }
        }

        return list;
    }

    public static (string Name, Type MemberType) RequireSingleMemberAccess<TEntity, TMember>(
        Expression<Func<TEntity, TMember>> expr)
    {
        if (expr.Body is not MemberExpression)
            throw new QueryConfigurationException("Expression must be a simple member access.");

        var chain = UnwrapMemberChain(expr.Body);
        if (chain.Count != 1)
            throw new QueryConfigurationException("Use RuleFor with UseConfiguration for nested complex member paths.");

        return chain[0];
    }

    public static bool IsCollectionType(Type t)
    {
        if (t == typeof(string) || t == typeof(byte[]))
            return false;

        if (t.IsArray)
            return true;

        foreach (var i in t.GetInterfaces())
            if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>))
                return true;

        return false;
    }

    public static Type GetEnumerableElementType(Type t)
    {
        if (t.IsArray)
            return t.GetElementType()!;

        var enumerable = t.GetInterfaces()
            .Concat(new[] { t })
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        if (enumerable is null)
            throw new QueryConfigurationException($"Type {t.Name} is not IEnumerable<>.");

        return enumerable.GetGenericArguments()[0];
    }
}
