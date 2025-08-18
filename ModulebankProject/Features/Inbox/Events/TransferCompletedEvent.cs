namespace ModulebankProject.Features.Inbox.Events;

public class TransferCompletedEvent : EventBase
{
    public Guid SourceAccountId { get; set; }
    public Guid DestinationAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public Guid TransferId { get; set; }
}