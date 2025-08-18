using MediatR;
using ModulebankProject.Features.Outbox;
using ModulebankProject.MbResult;
using ModulebankProject.PipelineBehaviors.Outbox;

namespace ModulebankProject.Features.Accounts.CreateAccount
{
    /// <summary>
    /// Command для создания нового счёта
    /// </summary>
    /// <param name="OwnerId">Владелец счёта</param>
    /// <param name="AccountType">Тип счёта</param>
    /// <param name="Currency">Валюта</param>
    /// <param name="InterestRate">Процентная ставка</param>
    /// <param name="CloseDate">Дата закрытия счёта</param>
    public record CreateAccountCommand (
        Guid OwnerId,
        AccountType AccountType,
        string Currency,
        decimal InterestRate,
        DateTime CloseDate) : IRequest<MbResult<AccountDto, ApiError>>, ICommandWithEvents
    {
        public List<OutboxMessage> Events { get; } = [];
    }
}