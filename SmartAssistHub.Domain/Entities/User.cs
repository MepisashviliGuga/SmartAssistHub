using SmartAssistHub.Domain.Common;
using SmartAssistHub.Domain.Enums;
using SmartAssistHub.Domain.Events;
using SmartAssistHub.Domain.ValueObjects;

namespace SmartAssistHub.Domain.Entities;

public class User : BaseEntity
{
    public Guid TenantId { get; private set; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public DateTime? EmailVerifiedAt { get; private set; }

    private readonly List<Conversation> _conversations = new();
    public IReadOnlyCollection<Conversation> Conversations =>
        _conversations.AsReadOnly();

    private User() { }

    public static User Create(
        Guid tenantId,
        string firstName,
        string lastName,
        string email,
        UserRole role = UserRole.Member)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        if (tenantId == Guid.Empty)
            throw new ArgumentException(
                "TenantId cannot be empty.", nameof(tenantId));

        var user = new User
        {
            TenantId = tenantId,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = Email.Create(email),
            Role = role,
            IsActive = true,
            IsEmailVerified = false,
            EmailVerifiedAt = null,
            LastLoginAt = null
        };

        user.RaiseDomainEvent(new UserCreatedEvent(
            user.Id,
            user.TenantId,
            user.Email.Value,
            user.Role.ToString()));

        return user;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        SetUpdated();
    }

    public void ChangeRole(UserRole newRole)
    {
        if (Role == UserRole.Owner && newRole != UserRole.Owner)
            throw new InvalidOperationException(
                "Cannot demote the owner without transferring ownership first.");

        var previousRole = Role;
        Role = newRole;
        RaiseDomainEvent(new UserRoleChangedEvent(
            Id,
            TenantId,
            previousRole.ToString(),
            newRole.ToString()));
        SetUpdated();
    }

    public void TransferOwnership(User newOwner)
    {
        ArgumentNullException.ThrowIfNull(newOwner);

        if (Role != UserRole.Owner)
            throw new InvalidOperationException(
                "Only an Owner can transfer ownership.");

        if (!newOwner.IsActive)
            throw new InvalidOperationException(
                "Cannot transfer ownership to an inactive user.");

        if (newOwner.TenantId != TenantId)
            throw new InvalidOperationException(
                "Cannot transfer ownership to a user " +
                "from a different tenant.");

        newOwner.ChangeRole(UserRole.Owner);
        Role = UserRole.Admin;
        SetUpdated();
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException(
                "User is already deactivated.");

        IsActive = false;
        RaiseDomainEvent(new UserDeactivatedEvent(Id, TenantId));
        SetUpdated();
    }

    public void VerifyEmail()
    {
        if (!IsActive)
            throw new InvalidOperationException(
                "Cannot verify email of an inactive user.");

        if (IsEmailVerified)
            throw new InvalidOperationException(
                "Email is already verified.");

        IsEmailVerified = true;
        EmailVerifiedAt = DateTime.UtcNow;
        RaiseDomainEvent(new UserEmailVerifiedEvent(Id, TenantId, Email.Value));
        SetUpdated();
    }

    public string FullName => $"{FirstName} {LastName}";
}