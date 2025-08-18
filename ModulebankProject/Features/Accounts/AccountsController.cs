using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModulebankProject.Features.Accounts.CheckAccountAvailability;
using ModulebankProject.Features.Accounts.CreateAccount;
using ModulebankProject.Features.Accounts.DeleteAccount;
using ModulebankProject.Features.Accounts.EditAccount;
using ModulebankProject.Features.Accounts.GetAccounts;
using ModulebankProject.Features.Accounts.GetAccountStatement;
namespace ModulebankProject.Features.Accounts;

/// <summary>
/// Контроллер счетов
/// </summary>
/// <param name="mediator"></param>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AccountsController(IMediator mediator) : Controller
{
    // ReSharper disable once ReplaceWithPrimaryConstructorParameter мне удобнее иметь readonly поле
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Создание счёта
    /// </summary>
    /// <param name="ownerId">Владелец счёта</param>
    /// <param name="command"></param>
    /// <returns></returns>
    [HttpPost("{ownerId:guid}/CreateAccount")]
    public async Task<ActionResult> CreateAccount(Guid ownerId, [FromBody] CreateAccountCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Decide(
            success: x => CreatedAtRoute("GetAccountStatement", new { ownerId = x!.OwnerId, accountId = x.Id }, x),
            failure: e => StatusCode(e!.StatusCode, new { Success = false, Error = e.GetResponse() }));
    }

    /// <summary>
    /// Редактирование параметров счёта
    /// </summary>
    /// <param name="ownerId">Владелец счёта</param>
    /// <param name="id">Номер счёта</param>
    /// <param name="command"></param>
    /// <returns></returns>
    [HttpPatch("{ownerId:guid}/{id:guid}")]
    public async Task<ActionResult<Account>> EditAccount(Guid ownerId, Guid id,
        [FromBody] EditAccountCommand command)
    {
        var result = await _mediator
            .Send(new EditAccountCommand(
                id,
                // ReSharper disable once UseWithExpressionToCopyRecord сокращение не читабельно
                command.Currency,
                command.InterestRate,
                command.CloseDate));
        return result.Decide(
            // ReSharper disable once ConvertClosureToMethodGroup с лямбда лучше понимаю код
            success: x => Ok(x),
            failure: e => StatusCode(e!.StatusCode, new { Success = false, Error = e.GetResponse() }));
    }

    /// <summary>
    /// Удаление счёта
    /// </summary>
    /// <param name="ownerId">Владелец счёта</param>
    /// <param name="id">Номер счёта</param>
    /// <returns></returns>
    [HttpDelete("{ownerId:guid}/{id:guid}")]
    public async Task<ActionResult<Guid>> DeleteAccount(Guid ownerId, Guid id)
    {
        var result = await _mediator.Send(new DeleteAccountCommand(id));
        return result.Decide(
            success: x => Ok(x),
            failure: e => StatusCode(e!.StatusCode, new { Success = false, Error = e.GetResponse() }));
    }

    /// <summary>
    /// Получить список счетов клиента
    /// </summary>
    /// <param name="ownerId">Владелец счетов</param>
    /// <returns></returns>
    [HttpGet("{ownerId:guid}")]
    public async Task<ActionResult<List<AccountDto>>> GetAccounts(Guid ownerId)
    {
        var result = await _mediator.Send(new GetAccountsRequest(ownerId));
        return result.Decide(
            // ReSharper disable once ConvertClosureToMethodGroup с лямбда лучше понимаю код
            success: x => Ok(x),
            failure: e => StatusCode(e!.StatusCode, new { Success = false, Error = e.GetResponse() }));
    }

    /// <summary>
    /// Получить выписку по счёту
    /// </summary>
    /// <param name="ownerId">Владелец счёта</param>
    /// <param name="accountId">Номер счёта</param>
    /// <param name="startRangeDate">С какого числа</param>
    /// <param name="endRangeDate">По какое</param>
    /// <returns></returns>
    [HttpGet("{ownerId:guid}/{accountId:guid}", Name = "GetAccountStatement")]
    public async Task<ActionResult<AccountStatementDto>> GetAccountStatement(Guid ownerId, Guid accountId,
        DateTime startRangeDate, DateTime endRangeDate)
    {
        var result = await _mediator.Send(new GetAccountStatementRequest(accountId, startRangeDate, endRangeDate));
        return result.Decide(
            // ReSharper disable once ConvertClosureToMethodGroup с лямбда лучше понимаю код
            success: x => Ok(x),
            failure: e => StatusCode(e!.StatusCode, new { Success = false, Error = e.GetResponse() }));
    }

    /// <summary>
    /// Проверить существование счёта
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("CheckAccountAvailability")]
    public async Task<ActionResult<bool>> CheckAccountAvailability(CheckAccountAvailabilityRequest request)
    {
        var result = await _mediator.Send(request);
        return Ok(result.Result);
    }
}