using MediatR;
using Microsoft.AspNetCore.Mvc;
using ModulebankProject.Features.Transactions.GetTransaction;
using ModulebankProject.Features.Transactions.RegisterTransaction;
using ModulebankProject.Features.Transactions.TransferTransaction;
using ModulebankProject.Infrastructure;

namespace ModulebankProject.Features.Transactions
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : Controller
    {
        private readonly IMediator _mediator;

        public TransactionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("{ownerId}/")]
        public async Task<ActionResult> RegisterTransaction(Guid ownerId, [FromBody] RegisterTransactionCommand command)
        {
            if (!AuthentificationService.IsAuthentificated(ownerId)) return Forbid();

            TransactionDto transaction = await _mediator.Send(command);
            return CreatedAtRoute("GetTransaction", new { id = transaction.Id }, transaction);
        }
        
        [HttpPatch("{ownerId}/{id}")]
        public async Task<ActionResult> TransferTransaction(Guid ownerId, Guid id)
        {
            if (!AuthentificationService.IsAuthentificated(ownerId)) return Forbid();

            var result = await _mediator.Send(new TransferTransactionCommand(id));
            if (result.Status == TransactionStatus.Error)
            {
                if (result.Description is 
                    "Transaction Not Found" or 
                    "Account Not Found" or 
                    "CounterpartyAccount Not Found") return NotFound(result.Description);
                
                if (result.Description == "Insufficient Funds") return Forbid(result.Description);
            }
            return Ok("Transaction Complete");
        }

        [HttpGet("{ownerId}/{id}", Name = "GetTransaction")]
        public async Task<ActionResult<Transaction>> GetTransaction(Guid ownerId, Guid id)
        {
            if (!AuthentificationService.IsAuthentificated(ownerId)) return Forbid();

            TransactionDto? transaction = await _mediator.Send(new GetTransactionRequest(id));
            if (transaction == null) return NotFound("Transaction Not Found");

            return Ok(transaction);
        }
    }
}