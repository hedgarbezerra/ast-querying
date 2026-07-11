namespace AstQuerying.Queries.Common.Exceptions;

/// <summary>
///     Categorizes a <see cref="QueryConfigurationException" /> for tooling and structured logs.
/// </summary>
public enum QueryConfigurationFailureKind
{
    DuplicateQueryConfigurationForEntity,
    CannotCreateConfigurationInstance,
    DefaultSortMissing,
    DuplicateFilterOperatorName,
    DuplicateFunctionName,
    RuleForMemberNotCollection,
    RuleForCollectionElementTypeMismatch,
    MemberPathUnsupportedExpression,
    MemberPathNotSimpleAccess,
    MemberPathNestedComplexNotSupported,
    TypeNotGenericEnumerable,
    HasAliasInvalidOnNavigation,
    FilteringOnlyValidOnScalar,
    SortingOnlyValidOnScalar,
    WithDefaultOnlyValidOnScalar,
    UseConfigurationOnlyValidOnNavigationClass,
    BuiltInFilterOperatorRegistrationIncomplete,
    BuiltInFunctionRegistrationIncomplete
}