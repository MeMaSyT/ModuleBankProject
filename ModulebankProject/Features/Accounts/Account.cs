using ModulebankProject.Features.Transactions;
using System.ComponentModel.DataAnnotations;

namespace ModulebankProject.Features.Accounts;

/// <summary>
/// Модель счёта
/// </summary>
public class Account
{
    // ReSharper disable once ConvertToPrimaryConstructor Не хочу использовать первичный конструктор
    public Account(
        Guid id,
        Guid ownerId,
        AccountType accountType,
        string currency,
        decimal balance,
        decimal interestRate,
        DateTime openDate,
        DateTime closeDate)
    {
        Id = id;
        OwnerId = ownerId;
        AccountType = accountType;
        Currency = currency;
        Balance = balance;
        InterestRate = interestRate;
        OpenDate = openDate;
        CloseDate = closeDate;
    }

    /// <summary>
    /// Номер счёта
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Владелец счёта
    /// </summary>
    public Guid OwnerId { get; set; }
    /// <summary>
    /// Тип счёта
    /// </summary>
    public AccountType AccountType { get; set; }
    /// <summary>
    /// Валюта
    /// </summary>
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Currency { get; set; }
    /// <summary>
    /// Текущий баланс
    /// </summary>
    public decimal Balance { get; set; }
    /// <summary>
    /// Процентная ставка
    /// </summary>
    public decimal InterestRate { get; set; }
    /// <summary>
    /// Дата открытия счёта
    /// </summary>
    public DateTime OpenDate { get; set; }
    /// <summary>
    /// Дата закрытия счёта
    /// </summary>
    public DateTime CloseDate { get; set; }

    /// <summary>
    /// Транзакции
    /// </summary>
    public bool Freezing { get; set; } = false;
    public List<Transaction> Transactions { get; set; } = [];

    [Timestamp]
    public uint Version { get; set; }
}