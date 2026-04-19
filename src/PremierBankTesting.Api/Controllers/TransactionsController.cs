using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PremierBankTesting.Contracts.Models;
using PremierBankTesting.Contracts.Models.Transactions;
using PremierBankTesting.Contracts.Paginations;
using PremierBankTesting.Contracts.Transactions.Requests.Commands;
using PremierBankTesting.Contracts.Transactions.Requests.Queries;

namespace PremierBankTesting.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Импорт транзакций из банка</summary>
    /// <remarks>
    /// Получает список последних транзакций через банковский API.
    /// Пользователи, отсутствующие в базе, создаются автоматически по email.
    /// Уже существующие транзакции (по Id) пропускаются — повторный вызов безопасен.
    /// </remarks>
    /// <response code="200">Возвращает количество импортированных транзакций</response>
    [HttpPost("import")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> Import(CancellationToken cancellationToken)
    {
        var imported = await _mediator.Send(new ImportTransactionsCommand(), cancellationToken);
        return Ok(new { imported });
    }

    /// <summary>Получить транзакции с фильтром по статусу обработки</summary>
    /// <remarks>
    /// Возвращает постраничный список транзакций.
    /// - isProcessed=false — необработанные (по умолчанию)
    /// - isProcessed=true — обработанные
    /// </remarks>
    /// <param name="isProcessed">Фильтр по статусу обработки (по умолчанию false)</param>
    /// <param name="page">Номер страницы (по умолчанию 1)</param>
    /// <param name="pageSize">Размер страницы (по умолчанию 20)</param>
    /// <param name="cancellationToken">Токен отмены запроса</param>
    /// <response code="200">Постраничный список транзакций</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TransactionModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] bool isProcessed = false,
        [FromQuery][Range(1, int.MaxValue)] int page = 1,
        [FromQuery][Range(1, 100)] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetTransactionsQuery(isProcessed, page, pageSize), cancellationToken);
        return Ok(result);
    }

    /// <summary>Пометить транзакцию как обработанную</summary>
    /// <remarks>
    /// После пометки транзакция учитывается в отчётах.
    /// Повторный вызов на уже обработанной транзакции безопасен.
    /// </remarks>
    /// <param name="id">Идентификатор транзакции (GUID)</param>
    /// <param name="cancellationToken">Токен отмены запроса</param>
    /// <response code="204">Транзакция успешно обработана</response>
    /// <response code="404">Транзакция с указанным Id не найдена</response>
    [HttpPost("{id:guid}/mark-as-processed")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsProcessed(Guid id, CancellationToken cancellationToken)
    {
        var success = await _mediator.Send(new MarkTransactionProcessedCommand(id), cancellationToken);
        return success ? NoContent() : NotFound();
    }
}
