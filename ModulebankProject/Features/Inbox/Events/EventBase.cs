namespace ModulebankProject.Features.Inbox.Events;

public abstract class EventBase
{
    /// <summary>
    /// Уникальный идентификатор события
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid EventId { get; set; }

    /// <summary>
    /// Дата и время возникновения события
    /// </summary>
    /// <example>2023-01-01T12:00:00Z</example>
    // ReSharper disable once UnusedMember.Global
    public DateTime OccurredAt { get; set; }

    public string Version { get; set; } = "v1";

}