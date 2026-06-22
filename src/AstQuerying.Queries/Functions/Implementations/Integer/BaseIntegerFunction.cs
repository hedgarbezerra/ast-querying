using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Functions.Contracts;

namespace AstQuerying.Queries.Functions.Implementations.Integer;

/// <inheritdoc />
public abstract class BaseIntegerFunction : BaseFunction<int>
{
    /// <summary>
    /// Centralized mapping of supported integer types to conversion functions.
    /// </summary>
    protected override Dictionary<Type, Func<int, object>> Converters => new()
    {
        [typeof(int)] = v => v,
        [typeof(uint)] = v => (uint)v,
        [typeof(long)] = v => (long)v,
        [typeof(ulong)] = v => (ulong)v,
        [typeof(short)] = v => (short)v,
        [typeof(ushort)] = v => (ushort)v,
        [typeof(byte)] = v => (byte)v,
        [typeof(sbyte)] = v => (sbyte)v,
        [typeof(Int128)] = v => (Int128)v,
        [typeof(UInt128)] = v => (UInt128)v,
        [typeof(nint)] = v => (nint)v,
        [typeof(nuint)] = v => (nuint)v
    };
}