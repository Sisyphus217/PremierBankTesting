using MediatR;
using PremierBankTesting.Contracts.Models.Transactions;

namespace PremierBankTesting.Contracts.Transactions.Requests.Queries;

public record GetTypeReportQuery : IRequest<IReadOnlyList<TransactionTypeReportModel>>;
