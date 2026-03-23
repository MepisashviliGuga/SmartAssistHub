using SmartAssistHub.Domain.Common;
using SmartAssistHub.Domain.Enums;
using SmartAssistHub.Domain.Events;
using SmartAssistHub.Domain.ValueObjects;

namespace SmartAssistHub.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; private set; } = null!;
    public Slug Slug { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public TenantPlan Plan { get; private set; }
    public int MonthlyTokenLimit { get; private set; }
    public int TokensUsedThisMonth { get; private set; }

    private readonly List<User> _users = new();
    public IReadOnlyCollection<User> Users => _users.AsReadOnly();

    private readonly List<Document> _documents = new();
    public IReadOnlyCollection<Document> Documents => _documents.AsReadOnly();

    private Tenant() { }

    public static Tenant Create(string name, Slug slug, TenantPlan plan)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var tenant = new Tenant
        {
            Name = name,
            Slug = slug,
            IsActive = true,
            Plan = plan,
            MonthlyTokenLimit = GetTokenLimitForPlan(plan),
            TokensUsedThisMonth = 0
        };
        tenant.RaiseDomainEvent(new TenantCreatedEvent(tenant.Id, tenant.Name, tenant.Slug.Value));
        return tenant;
    }

    public void AddDocument(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (!IsActive)
            throw new InvalidOperationException(
                "Cannot add documents to an inactive tenant.");

        _documents.Add(document);
        SetUpdated();
    }

    public void AddUser(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (!IsActive)
            throw new InvalidOperationException(
                "Cannot add users to an inactive tenant.");

        _users.Add(user);
        SetUpdated();
    }

    public void RecordTokenUsage(int tokensUsed)
    {
        if (tokensUsed <= 0)
            throw new ArgumentException(
                "Token usage must be positive.", nameof(tokensUsed));

        TokensUsedThisMonth += tokensUsed;
        SetUpdated();
    }

    public bool HasTokenBudgetRemaining()
    {
        return TokensUsedThisMonth < MonthlyTokenLimit;
    }

    public void ResetMonthlyUsage()
    {
        TokensUsedThisMonth = 0;
        SetUpdated();
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException(
                "Tenant is already deactivated.");

        IsActive = false;
        RaiseDomainEvent(new TenantDeactivatedEvent(Id));
        SetUpdated();
    }

    private static int GetTokenLimitForPlan(TenantPlan plan) => plan switch
    {
        TenantPlan.Free => 100_000,
        TenantPlan.Pro => 1_000_000,
        TenantPlan.Enterprise => 10_000_000,
        _ => throw new ArgumentOutOfRangeException(nameof(plan))
    };
}