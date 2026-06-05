using AstQuerying.Queries.Configuration;
using AstQuerying.Queries.Filters.Implementations;

namespace AstQuerying.Queries.Tests.Support;

public sealed class PersonQueryConfiguration : QueryConfiguration<Person>
{
    protected override void Configure()
    {
        RuleFor(p => p.Id).Filtering(f => f.Operators(o => o.Allow<EqualOperator>()));
        RuleFor(p => p.Name)
            .Filtering(f => f.Operators(o =>
                o.Allow<EqualOperator>().Allow<ContainsOperator>().Allow<StartsWithOperator>()))
            .Sorting(s => s.AllowAscending().AllowDescending());
        RuleFor(p => p.Age)
            .Filtering(f => f.Operators(o => o.Allow<EqualOperator>().Allow<GreaterThanOperator>()))
            .Sorting(s => s.AllowAscending().AllowDescending());
    }
}
