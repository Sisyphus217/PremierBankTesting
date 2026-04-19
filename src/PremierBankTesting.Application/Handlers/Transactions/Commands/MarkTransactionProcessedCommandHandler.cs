using MediatR;
using Microsoft.EntityFrameworkCore;
using PremierBankTesting.Application.Abstractions.Persistence;
using PremierBankTesting.Contracts.Transactions.Requests.Commands;

namespace PremierBankTesting.Application.Handlers.Transactions.Commands;

internal sealed class MarkTransactionProcessedCommandHandler : IRequestHandler<MarkTransactionProcessedCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public MarkTransactionProcessedCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<bool> Handle(MarkTransactionProcessedCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken);

        if (transaction is null)
            return false;

        transaction.MarkAsProcessed();
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}