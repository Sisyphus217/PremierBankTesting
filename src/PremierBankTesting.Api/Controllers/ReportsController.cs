using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PremierBankTesting.Contracts.Models.Transactions;
using PremierBankTesting.Contracts.Models.Users;
using PremierBankTesting.Contracts.Transactions.Requests.Queries;
using PremierBankTesting.Contracts.Users.Requests.Queries;

namespace PremierBankTesting.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Отчёт по пользователям за последний месяц</summary>
    /// <remarks>
    /// Возвращает список пользователей с суммой и количеством их обработанных транзакций
    /// за последние 30 дней. Учитываются только транзакции с IsProcessed = true.
    /// </remarks>
    /// <response code="200">Список пользователей с агрегированными данными по транзакциям</response>
    [HttpGet("users")]
    [ProducesResponseType(typeof(IReadOnlyList<UserReportModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserReport(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUserReportQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>Отчёт по типам операций</summary>
    /// <remarks>
    /// Группирует все обработанные транзакции по типу операции (полю Comment).
    /// Возвращает сумму и количество транзакций для каждого типа.
    /// Учитываются только транзакции с IsProcessed = true.
    /// </remarks>
    /// <response code="200">Список типов операций с агрегированными данными</response>
    [HttpGet("types")]
    [ProducesResponseType(typeof(IReadOnlyList<TransactionTypeReportModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTypeReport(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTypeReportQuery(), cancellationToken);
        return Ok(result);
    }
}
