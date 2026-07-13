using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="ISlackIntegrationRepository"/> (single config row).</summary>
internal sealed class SlackIntegrationRepository(ApplicationDbContext context) : ISlackIntegrationRepository
{
    public Task<SlackIntegration?> GetAsync(CancellationToken ct = default) =>
        context.SlackIntegrations.OrderBy(s => s.CreatedAt).FirstOrDefaultAsync(ct);

    public async Task AddAsync(SlackIntegration integration, CancellationToken ct = default) =>
        await context.SlackIntegrations.AddAsync(integration, ct);

    public void Update(SlackIntegration integration) => context.SlackIntegrations.Update(integration);

    public void Remove(SlackIntegration integration) => context.SlackIntegrations.Remove(integration);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
}
