using MediatR;
using Microsoft.EntityFrameworkCore;
using PremierBankTesting.Application.Abstractions.Persistence;
using PremierBankTesting.Contracts.Models.Transactions;
using PremierBankTesting.Contracts.Paginations;
using PremierBankTesting.Contracts.Transactions.Requests.Queries;

namespace PremierBankTesting.Application.Handlers.Transactions.Queries;

internal sealed class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, PagedResult<TransactionModel>>
{
    private readonly IApplicationDbContext _context;

    public GetTransactionsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PagedResult<TransactionModel>> Handle(
        GetTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Transactions
            .AsNoTracking()
            .Where(t => t.IsProcessed == request.IsProcessed);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.Timestamp)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new TransactionModel(
                t.Id,
                t.Amount,
                t.Comment,
                t.Timestamp,
                t.IsProcessed,
                t.User.Email))
            .ToListAsync(cancellationToken);

        return new PagedResult<TransactionModel>(items, totalCount, request.Page, request.PageSize);
    }
}