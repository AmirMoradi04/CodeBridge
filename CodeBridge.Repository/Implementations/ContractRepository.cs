using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Enums;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeBridge.Repository.Implementations;

/// <summary>ریپازیتوری قراردادها؛ فقط دسترسی به داده.</summary>
public sealed class ContractRepository : GenericRepository<Contract>, IContractRepository
{
    public ContractRepository(AppDbContext context) : base(context)
    {
    }

    public Task<Contract?> GetWithDetailsAsync(
        int contractId,
        CancellationToken cancellationToken = default)
        => DbSet
            .AsNoTracking()
            .Include(contract => contract.Project)
            .Include(contract => contract.Developer)
            .Include(contract => contract.Employer)
            .Include(contract => contract.Admin)
            .FirstOrDefaultAsync(contract => contract.Id == contractId, cancellationToken);

    public Task<Contract?> GetByProjectIdAsync(int projectId, CancellationToken cancellationToken = default)
        => DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(contract => contract.ProjectId == projectId, cancellationToken);

    public async Task<IReadOnlyList<Contract>> GetByDeveloperIdAsync(
        int developerId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(contract => contract.DeveloperId == developerId)
            .OrderByDescending(contract => contract.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Contract>> GetByEmployerIdAsync(
        int employerId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(contract => contract.EmployerId == employerId)
            .OrderByDescending(contract => contract.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Contract>> GetActiveAsync(CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(contract => contract.Status == ContractStatus.Active)
            .OrderByDescending(contract => contract.CreatedAt)
            .ToListAsync(cancellationToken);
}
