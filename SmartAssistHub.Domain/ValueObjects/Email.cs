using System.Text.RegularExpressions;

namespace SmartAssistHub.Domain.ValueObjects;

public record Email
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Trim().ToLowerInvariant();

        if (!EmailRegex.IsMatch(normalized))
            throw new ArgumentException(
                $"'{value}' is not a valid email address.", nameof(value));

        return new Email(normalized);
    }

    public override string ToString() => Value;
}