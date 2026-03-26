namespace SmartAssistHub.Application.Features.Documents.Common;

public record DocumentDto(
    Guid Id,
    Guid TenantId,
    Guid UploadedByUserId,
    string FileName,
    string ContentType,
    string FileSize,
    string Status,
    string? FailureReason,
    int TotalChunks,
    int EmbeddedChunks,
    DateTime? ProcessedAt,
    DateTime CreatedAt);