using MediatR;
using Microsoft.EntityFrameworkCore;
using PremierBankTesting.Application.Abstractions.Bank;
using PremierBankTesting.Application.Abstractions.Persistence;
using PremierBankTesting.Contracts.Transactions.Requests.Commands;
using PremierBankTesting.Domain.Entities;

namespace PremierBankTesting.Application.Handlers.Transactions.Commands;

internal sealed class ImportTransactionsCommandHandler : IRequestHandler<ImportTransactionsCommand, int>
{
    private readonly IBankApiClient _bankApiClient;
    private readonly IApplicationDbContext _context;

    public ImportTransactionsCommandHandler(IBankApiClient bankApiClient, IApplicationDbContext context)
    {
        _bankApiClient = bankApiClient;
        _context = context;
    }

    public async Task<int> Handle(ImportTransactionsCommand request, CancellationToken cancellationToken)
    {
        var bankTransactions = await _bankApiClient.GetRecentTransactionsAsync(cancellationToken);

        if (bankTransactions.Count == 0)
            return 0;

        var incomingIds = bankTransactions.Select(t => t.Id).ToList();

        var existingIds = (await _context.Transactions
                .Where(t => incomingIds.Contains(t.Id))
                .Select(t => t.Id)
                .ToListAsync(cancellationToken))
            .ToHashSet();

        var newBankTransactions = bankTransactions.Where(t => !existingIds.Contains(t.Id)).ToList();
        if (newBankTransactions.Count == 0)
            return 0;

        var emails = newBankTransactions.Select(t => t.UserEmail.ToLowerInvariant()).Distinct().ToList();

        var existingUsers = await _context.Users
            .Where(u => emails.Contains(u.Email))
            .ToDictionaryAsync(u => u.Email, cancellationToken);

        var newUsers = emails
            .Where(e => !existingUsers.ContainsKey(e))
            .Select(User.Create)
            .ToList();

        if (newUsers.Count > 0)
        {
            _context.Users.AddRange(newUsers);
            foreach (var user in newUsers)
                existingUsers[user.Email] = user;
        }

        var newTransactions = newBankTransactions
            .Select(bt =>
                Transaction.Create(bt.Id, bt.Amount, bt.Comment, bt.Timestamp,
                    existingUsers[bt.UserEmail.ToLowerInvariant()].Id))
            .ToList();

        _context.Transactions.AddRange(newTransactions);
        await _context.SaveChangesAsync(cancellationToken);
        return newTransactions.Count;
    }
}