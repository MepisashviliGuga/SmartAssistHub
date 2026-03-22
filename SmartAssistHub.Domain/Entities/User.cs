using SmartAssistHub.Domain.Common;
using SmartAssistHub.Domain.Enums;
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

        return new User
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

        Role = newRole;
        SetUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
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
        SetUpdated();
    }

    public string FullName => $"{FirstName} {LastName}";
}