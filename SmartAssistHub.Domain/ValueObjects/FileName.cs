namespace SmartAssistHub.Domain.ValueObjects;

public record FileName
{
    private static readonly HashSet<string> AllowedExtensions = new(
        StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".docx", ".txt"
    };

    public string Value { get; }
    public string Extension { get; }

    private FileName(string value)
    {
        Value = value;
        Extension = Path.GetExtension(value);
    }

    public static FileName Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var trimmed = value.Trim();

        if (trimmed.Contains("..") ||
            trimmed.Contains('/') ||
            trimmed.Contains('\\'))
            throw new ArgumentException(
                "File name contains invalid characters.", nameof(value));

        var extension = Path.GetExtension(trimmed);

        if (string.IsNullOrWhiteSpace(extension))
            throw new ArgumentException(
                "File name must have an extension.", nameof(value));

        if (!AllowedExtensions.Contains(extension))
            throw new ArgumentException(
                $"Extension '{extension}' is not supported. " +
                $"Allowed: {string.Join(", ", AllowedExtensions)}",
                nameof(value));

        return new FileName(trimmed);
    }
    internal static FileName FromDatabase(string value)
    {
        return new FileName(value);
    }
    public override string ToString() => Value;
}