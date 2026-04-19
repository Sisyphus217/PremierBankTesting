using MediatR;
using PremierBankTesting.Contracts.Models.Transactions;
using PremierBankTesting.Contracts.Paginations;

namespace PremierBankTesting.Contracts.Transactions.Requests.Queries;

public record GetTransactionsQuery(
    bool IsProcessed = false,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<TransactionModel>>;
