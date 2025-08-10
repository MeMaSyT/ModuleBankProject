using System.ComponentModel.DataAnnotations;

namespace ModulebankProject.Features.Transactions
{
    /// <summary>
    /// Модель транзакции
    /// </summary>
    public class Transaction
    {
        // ReSharper disable once ConvertToPrimaryConstructor не хочу первичный конструктор
        public Transaction(
            Guid id,
            Guid accountId,
            Guid? counterpartyAccountId,
            decimal amount,
            string currency,
            TransactionType transactionType,
            string description,
            DateTime time,
            TransactionStatus transactionStatus)
        {
            Id = id;
            AccountId = accountId;
            CounterpartyAccountId = counterpartyAccountId;
            Amount = amount;
            Currency = currency;
            TransactionType = transactionType;
            Description = description;
            Time = time;
            TransactionStatus = transactionStatus;
        }

        /// <summary>
        /// Номер транзакции
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Номер счёта
        /// </summary>
        public Guid AccountId { get; set; }
        /// <summary>
        /// Номер счёта контрагента
        /// </summary>
        public Guid? CounterpartyAccountId { get; set; }
        /// <summary>
        /// Сумма транзакции
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// Валюта
        /// </summary>
        // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
        public string Currency { get; set; }
        /// <summary>
        /// Тип транзакции
        /// </summary>
        public TransactionType TransactionType { get; set; }
        /// <summary>
        /// Описание
        /// </summary>
        // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
        public string Description { get; set; }
        /// <summary>
        /// Время транзакции
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// Статус транзакции
        /// </summary>
        public TransactionStatus TransactionStatus { get; set; }

        [Timestamp]
        public uint Version { get; set; }
    }
}