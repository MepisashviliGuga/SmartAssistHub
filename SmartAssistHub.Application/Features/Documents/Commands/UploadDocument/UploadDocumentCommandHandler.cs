using MediatR;
using Microsoft.Extensions.Logging;
using SmartAssistHub.Application.Common.Exceptions;
using SmartAssistHub.Application.Common.Interfaces;
using SmartAssistHub.Application.Common.Interfaces.Repositories;
using SmartAssistHub.Application.Common.Interfaces.Services;
using SmartAssistHub.Domain.Entities;

namespace SmartAssistHub.Application.Features.Documents.Commands.UploadDocument;

public class UploadDocumentCommandHandler
    : IRequestHandler<UploadDocumentCommand, UploadDocumentResult>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBlobStorageService _blobStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UploadDocumentCommandHandler> _logger;

    public UploadDocumentCommandHandler(
        IDocumentRepository documentRepository,
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IBlobStorageService blobStorageService,
        ILogger<UploadDocumentCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository;
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _blobStorageService = blobStorageService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UploadDocumentResult> Handle(
        UploadDocumentCommand command,
        CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository
            .GetByIdAsync(command.TenantId, cancellationToken);

        if (tenant is null)
            throw new NotFoundException(
                nameof(tenant), command.TenantId);

        if (!tenant.IsActive)
            throw new ForbiddenException(
                "Cannot upload documents to an inactive tenant.");

        var user = await _userRepository
            .GetByIdAsync(command.UploadedByUserId, cancellationToken);

        if (user is null)
            throw new NotFoundException(
                nameof(user), command.UploadedByUserId);

        if (user.TenantId != command.TenantId)
            throw new ForbiddenException(
                "User does not belong to this tenant.");

        if (!user.IsActive)
            throw new ForbiddenException(
                "Inactive users cannot upload documents.");

        var blobPath = await _blobStorageService.UploadAsync(
            command.FileStream,
            command.FileName,
            command.ContentType,
            command.TenantId,
            cancellationToken);

        try
        {
            var document = Document.Create(
                command.TenantId,
                command.UploadedByUserId,
                command.FileName,
                command.ContentType,
                command.FileSizeBytes,
                blobPath);

            await _documentRepository.AddAsync(document, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new UploadDocumentResult(
                document.Id,
                document.FileName.Value,
                document.Status.ToString());
        }
        catch
        {
            try
            {

                await _blobStorageService.DeleteAsync(blobPath, cancellationToken);
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex,
                    "Failed to delete orphaned blob {BlobPath}. " +
                    "Nightly cleanup job will handle this.",
                    blobPath);
            }
            throw;
        }
    }
}