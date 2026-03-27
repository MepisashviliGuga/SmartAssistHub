using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace SmartAssistHub.Domain.ValueObjects;

public record Slug
{
    public string Value { get; } = null!;

    private Slug(string value)
    {
        Value = value;
    }
    private Slug() { }

    public static Slug Generate(string input, int maxLength = 100)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        var slug = input.ToLowerInvariant().Trim();
        slug = RemoveDiacritics(slug);
        slug = Regex.Replace(slug, @"[^\p{L}\p{Nd}\s-]", " ");
        slug = Regex.Replace(slug, @"\s+", " ").Trim();
        slug = slug.Replace(' ', '-');
        slug = Regex.Replace(slug, @"-{2,}", "-");
        slug = slug.Trim('-');

        if (slug.Length > maxLength)
            slug = slug[..maxLength].Trim('-');

        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException(
                "Input produces an empty slug.", nameof(input));

        return new Slug(slug);
    }

    private static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c)
                != UnicodeCategory.NonSpacingMark)
                builder.Append(c);
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    public override string ToString() => Value;
}