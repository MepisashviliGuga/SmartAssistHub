using SmartAssistHub.Domain.Common;
using SmartAssistHub.Domain.Enums;
using SmartAssistHub.Domain.ValueObjects;

namespace SmartAssistHub.Domain.Entities;

public class User : BaseEntity
{
    public Guid TenantId { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public Email Email { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    //private readonly List<Conversation> _conversations = new();
    //public IReadOnlyCollection<Conversation> Conversations =>
    //    _conversations.AsReadOnly();

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

    public string FullName => $"{FirstName} {LastName}";
}