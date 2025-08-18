namespace ModulebankProject.Features.Accounts;

/// <summary>
/// Dto, содержащее информацию о счете
/// </summary>
/// <param name="Id">Номер счёта</param>
/// <param name="OwnerId">Владелец счёта</param>
/// <param name="AccountType">Тип счёта</param>
/// <param name="Currency">Валюта</param>
/// <param name="Balance">Текущий баланс</param>
/// <param name="InterestRate">Процентная ставка</param>
/// <param name="OpenDate">Дата открытия счёта</param>
/// <param name="CloseDate">Дата закрытия счёта</param>
public record AccountDto(
    Guid Id,
    Guid OwnerId,
    AccountType AccountType,
    string Currency,
    decimal Balance,
    decimal InterestRate,
    DateTime OpenDate,
    DateTime CloseDate
);