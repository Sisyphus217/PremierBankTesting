using MediatR;
using Microsoft.EntityFrameworkCore;
using PremierBankTesting.Application.Abstractions.Persistence;
using PremierBankTesting.Contracts.Models.Transactions;
using PremierBankTesting.Contracts.Transactions.Requests.Queries;

namespace PremierBankTesting.Application.Handlers.Transactions.Queries;

internal sealed class GetTypeReportQueryHandler : IRequestHandler<GetTypeReportQuery, IReadOnlyList<TransactionTypeReportModel>>
{
    private readonly IApplicationDbContext _context;

    public GetTypeReportQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<TransactionTypeReportModel>> Handle(
        GetTypeReportQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Transactions
            .AsNoTracking()
            .Where(t => t.IsProcessed)
            .GroupBy(t => t.Comment)
            .OrderByDescending(g => g.Sum(t => t.Amount))
            .Select(g => new TransactionTypeReportModel(g.Key, g.Sum(t => t.Amount), g.Count()))
            .ToListAsync(cancellationToken);
    }
}
