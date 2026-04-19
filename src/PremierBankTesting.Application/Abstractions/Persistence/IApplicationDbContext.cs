using Microsoft.EntityFrameworkCore;
using PremierBankTesting.Domain.Entities;

namespace PremierBankTesting.Application.Abstractions.Persistence;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Transaction> Transactions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
