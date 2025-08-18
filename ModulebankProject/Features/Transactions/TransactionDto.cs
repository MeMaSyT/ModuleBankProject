namespace ModulebankProject.Features.Transactions;

/// <summary>
/// Dto, содержащее информацию о транзакции
/// </summary>
/// <param name="Id">Номер транзакции</param>
/// <param name="AccountId">Номер счёта</param>
/// <param name="CounterpartyAccountId">Номер счёта контрагента</param>
/// <param name="Amount">Сумма транзакции</param>
/// <param name="Currency">Валюта</param>
/// <param name="TransactionType">Тип транзакции</param>
/// <param name="Description">Описание</param>
/// <param name="Time">Время транзакции</param>
/// <param name="TransactionStatus">Статус транзакции</param>
public record TransactionDto(
    Guid Id,
    Guid AccountId,
    Guid CounterpartyAccountId,
    decimal Amount,
    string Currency,
    TransactionType TransactionType,
    string Description,
    DateTime Time,
    TransactionStatus TransactionStatus
);