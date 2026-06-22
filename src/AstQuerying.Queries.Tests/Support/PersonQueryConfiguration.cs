using AstQuerying.Queries.Configuration;
using AstQuerying.Queries.Filters.Implementations;
using AstQuerying.Queries.Filters.Operators;
using AstQuerying.Queries.Sorting.Dtos;

namespace AstQuerying.Queries.Tests.Support;

public sealed class PersonQueryConfiguration : QueryConfiguration<Person>
{
    protected override void Configure()
    {
        DefaultSorting(new SortFieldDto { Field = "Name", Direction = SortDirection.Ascending });

        RuleFor(p => p.Id)
            .Filtering(f => f.Operators(o => 
                o.Allow<EqualOperator>()));
        RuleFor(p => p.Name)
            .Filtering(f => f.Operators(o =>
                o.Allow<EqualOperator>().Allow<ContainsOperator>().Allow<StartsWithOperator>()))
            .Sorting(s => s.AllowAscending().AllowDescending());
        RuleFor(p => p.Age)
            .Filtering(f => f.Operators(o =>
                o.Allow<EqualOperator>().Allow<GreaterThanOperator>().Allow<GreaterThanOrEqualOperator>()))
            .Sorting(s => s.AllowAscending().AllowDescending());
        RuleFor(p => p.BirthDate)
            .Filtering(f => f.Operators(o =>
                o.Allow<EqualOperator>().Allow<GreaterThanOperator>().Allow<GreaterThanOrEqualOperator>()));
        RuleFor(p => p.Salary)
            .Filtering(f => f.Operators(o =>
                o.Allow<EqualOperator>().Allow<GreaterThanOperator>().Allow<GreaterThanOrEqualOperator>()));
    }
}
