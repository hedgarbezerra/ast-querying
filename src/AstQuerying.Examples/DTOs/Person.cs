using AstQuerying.Queries.Metadata.Attributes;

namespace AstQuerying.Examples.DTOs;

/// <summary>
///     Sample queryable entity used by the examples project.
/// </summary>
[QueryableEntity]
[DefaultSort("""
[
  { "field": "LastName", "direction": "asc" },
  { "field": "FirstName", "direction": "desc" }
]
""")]
public class Person
{
    /// <summary>
    ///     Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the first name.
    /// </summary>
    public string FirstName { get; set; } = "";

    /// <summary>
    ///     Gets or sets the last name.
    /// </summary>
    public string LastName { get; set; } = "";

    /// <summary>
    ///     Gets or sets the age in years.
    /// </summary>
    public int Age { get; set; }

    /// <summary>
    ///     Gets or sets the email address.
    /// </summary>
    public string Email { get; set; } = "";

    /// <summary>
    ///     Gets or sets whether the person is active.
    /// </summary>
    public bool IsActive { get; set; }
}
