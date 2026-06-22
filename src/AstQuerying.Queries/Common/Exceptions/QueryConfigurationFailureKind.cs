namespace AstQuerying.Queries.Common.Exceptions;

/// <summary>
/// Categorizes a <see cref="QueryConfigurationException"/> for tooling and structured logs.
/// </summary>
public enum QueryConfigurationFailureKind
{
    /// <summary>Unspecified failure kind.</summary>
    Unspecified = 0,

    DuplicateQueryConfigurationForEntity,
    CannotCreateConfigurationInstance,
    DefaultSortMissing,
    DuplicateFilterOperatorName,
    DuplicateFunctionName,
    RuleForMemberNotCollection,
    RuleForCollectionElementTypeMismatch,
    ConfigureProducedNoRules,
    RuleForMemberIsCollection,
    RuleForCollectionMemberNotCollection,
    RuleForCollectionTypeMismatch,
    NestedCollectionNotSupported,
    BeginCollectionElementTypeMismatch,
    PublicPropertyMissingRule,
    EmptyDraftsAfterValidation,
    DuplicatePropertyPath,
    DuplicateAlias,
    AliasConflictsWithPath,
    DefaultValueIncompatibleWithLeafType,
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

    BuiltInFunctionRegistrationIncomplete,
}
