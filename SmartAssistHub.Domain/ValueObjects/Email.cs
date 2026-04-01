using System.Text.RegularExpressions;

namespace SmartAssistHub.Domain.ValueObjects;

public record Email
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; } = null!;

    private Email(string value)
    {
        Value = value;
    }

    private Email() { }
    
    public static Email Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Trim().ToLowerInvariant();

        if (!EmailRegex.IsMatch(normalized))
            throw new ArgumentException(
                $"'{value}' is not a valid email address.", nameof(value));

        return new Email(normalized);
    }

    public static string Normalize(string value)
    {
        return value.Trim().ToLowerInvariant();
    }

    public override string ToString() => Value;
}