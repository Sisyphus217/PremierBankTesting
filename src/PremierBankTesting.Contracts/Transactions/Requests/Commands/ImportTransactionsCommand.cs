using MediatR;

namespace PremierBankTesting.Contracts.Transactions.Requests.Commands;

public record ImportTransactionsCommand : IRequest<int>;
