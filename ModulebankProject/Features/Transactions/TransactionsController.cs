using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModulebankProject.Features.Transactions.GetTransaction;
using ModulebankProject.Features.Transactions.RegisterTransaction;
using ModulebankProject.Features.Transactions.TransferTransaction;

namespace ModulebankProject.Features.Transactions
{
    /// <summary>
    /// Контроллер транзакций
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class TransactionsController : Controller
    {
        private readonly IMediator _mediator;

        // ReSharper disable once ConvertToPrimaryConstructor не хочу первичный конструктор
        public TransactionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Регистрация транзакции
        /// </summary>
        /// <param name="ownerId">Владелец транзакции</param>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost("{ownerId}/")]
        public async Task<ActionResult> RegisterTransaction(Guid ownerId, [FromBody] RegisterTransactionCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Decide(
                success: x => CreatedAtRoute("GetTransaction", new { ownerId, id = x!.Id }, x),
                failure: e => StatusCode(e!.StatusCode, new { Success = false, Error = e.GetResponse() }));
        }
        
        /// <summary>
        /// Проведение транзакции
        /// </summary>
        /// <param name="ownerId">Владелец транзакции</param>
        /// <param name="id">Номер транзакции</param>
        /// <returns></returns>
        [HttpPatch("{ownerId}/{id}")]
        public async Task<ActionResult> TransferTransaction(Guid ownerId, Guid id)
        {
            var result = await _mediator.Send(new TransferTransactionCommand(id));

            return result.Decide(
                // ReSharper disable once ConvertClosureToMethodGroup решарпер портит читабельность
                success: x => Ok(x),
                failure: e => StatusCode(e!.StatusCode, new { Success = false, Error = e.GetResponse()}));
        }

        /// <summary>
        /// Получить транзакцию
        /// </summary>
        /// <param name="ownerId">Владелец транзакции</param>
        /// <param name="id">Номер транзакции</param>
        /// <returns></returns>
        [HttpGet("{ownerId}/{id}", Name = "GetTransaction")]
        public async Task<ActionResult<Transaction>> GetTransaction(Guid ownerId, Guid id)
        {
            var result = await _mediator.Send(new GetTransactionRequest(id));
            return result.Decide(
                // ReSharper disable once ConvertClosureToMethodGroup с лямбда удобнее
                success: x => Ok(x),
                failure: e => StatusCode(e!.StatusCode, new { Success = false, Error = e.GetResponse() }));
        }
    }
}