namespace ModulebankProject.Features.Inbox.Events;

public class InterestAccruedEvent : EventBase
{
    public Guid AccountId { get; set; }
    public DateTime PeriodFrom { get; set; }
    public DateTime PeriodTo { get; set; }
}