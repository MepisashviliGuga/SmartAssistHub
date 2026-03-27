namespace SmartAssistHub.Domain.ValueObjects;

public record FileSize
{
    private const long MaxAllowedBytes = 100 * 1024 * 1024; // 100MB

    public long Bytes { get; }

    private FileSize(long bytes) => Bytes = bytes;
    private FileSize() { }
    public static FileSize Create(long bytes)
    {
        if (bytes <= 0)
            throw new ArgumentException(
                "File size must be greater than zero.", nameof(bytes));

        if (bytes > MaxAllowedBytes)
            throw new ArgumentException(
                $"File size exceeds the maximum allowed size of 100MB.",
                nameof(bytes));

        return new FileSize(bytes);
    }

    public double ToKilobytes() => Math.Round(Bytes / 1024.0, 2);
    public double ToMegabytes() => Math.Round(Bytes / (1024.0 * 1024.0), 2);

    public override string ToString() =>
        Bytes switch
        {
            < 1024 => $"{Bytes} B",
            < 1024 * 1024 => $"{ToKilobytes()} KB",
            _ => $"{ToMegabytes()} MB"
        };
}