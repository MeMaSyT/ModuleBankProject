using MediatR;
using ModulebankProject.MbResult;

namespace ModulebankProject.Features.Transactions.RegisterTransaction;

/// <summary>
/// Command для регистрации транзакции
/// </summary>
/// <param name="AccountId">Номер счёта</param>
/// <param name="CounterpartyAccountId">Номер счёта контрагента</param>
/// <param name="Amount">Сумма транзакции</param>
/// <param name="Currency">Валюта</param>
/// <param name="TransactionType">Тип транзакции</param>
/// <param name="Description">Описание</param>
public record RegisterTransactionCommand(
    Guid AccountId,
    Guid? CounterpartyAccountId,
    decimal Amount,
    string Currency,
    TransactionType TransactionType,
    string Description) : IRequest<MbResult<TransactionDto, ApiError>>;