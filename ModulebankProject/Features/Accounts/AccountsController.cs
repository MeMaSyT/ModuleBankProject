using MediatR;
using Microsoft.AspNetCore.Mvc;
using ModulebankProject.Features.Accounts.CheckAccountAvailability;
using ModulebankProject.Features.Accounts.CreateAccount;
using ModulebankProject.Features.Accounts.DeleteAccount;
using ModulebankProject.Features.Accounts.EditAccount;
using ModulebankProject.Features.Accounts.GetAccounts;
using ModulebankProject.Features.Accounts.GetAccountStatement;
using ModulebankProject.Infrastructure;

namespace ModulebankProject.Features.Accounts
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController(IMediator mediator) : Controller
    {
        private readonly IMediator _mediator = mediator;

        [HttpPost("{ownerId}/CreateAccount")]
        public async Task<ActionResult> CreateAccount(Guid ownerId, [FromBody] CreateAccountCommand command)
        {
            if (!AuthentificationService.IsAuthentificated(ownerId)) return Forbid();

            AccountDto account = await _mediator.Send(command);
            return CreatedAtRoute("GetAccountStatement", new { ownerId = account.OwnerId, accountId = account.Id },
                account);
        }

        [HttpPatch("{ownerId}/{id}")]
        public async Task<ActionResult<Account>> EditAccount(Guid ownerId, Guid id, [FromBody] EditAccountCommand command)
        {
            if (!AuthentificationService.IsAuthentificated(ownerId)) return Forbid();

            AccountDto? account = await _mediator
                .Send(new EditAccountCommand(
                    id,
                    command.Сurrency,
                    command.InterestRate,
                    command.CloseDate));
            if (account == null) return NotFound("Account Not Found");
            return Ok(account);
        }

        [HttpDelete("{ownerId}/{id}")]
        public async Task<ActionResult<Guid>> DeleteAccount(Guid ownerId, Guid id)
        {
            if (!AuthentificationService.IsAuthentificated(ownerId)) return Forbid();

            Guid deletedAccountId = await _mediator.Send(new DeleteAccountCommand(id));
            if (deletedAccountId == Guid.Empty) return NotFound("Account Not Found");
            return Ok(deletedAccountId);
        }

        [HttpGet("{ownerId}")]
        public async Task<ActionResult<List<AccountDto>>> GetAccounts(Guid ownerId)
        {
            if (!AuthentificationService.IsAuthentificated(ownerId)) return Forbid();

            List<AccountDto> accounts = await _mediator.Send(new GetAccountsRequest(ownerId));
            return Ok(accounts);
        }

        [HttpGet("{ownerId}/{accountId}", Name = "GetAccountStatement")]
        public async Task<ActionResult<AccountStatementDto>> GetAccountStatement(Guid ownerId, Guid accountId,
            DateTime startRangeDate, DateTime endRangeDate)
        {
            if (!AuthentificationService.IsAuthentificated(ownerId)) return Forbid();

            AccountStatementDto? accountStatement =
                await _mediator.Send(new GetAccountStatementRequest(accountId, startRangeDate, endRangeDate));
            if (accountStatement == null) return NotFound("Account Not Found");
            return Ok(accountStatement);
        }

        [HttpGet("CheckAccountAvailability")]
        public async Task<ActionResult<bool>> CheckAccountAvailability(
            [FromBody] CheckAccountAvailabilityRequest request)
        {
            bool availability = await _mediator.Send(request);
            return Ok(availability);
        }
    }
}