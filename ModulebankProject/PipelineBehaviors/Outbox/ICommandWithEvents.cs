using ModulebankProject.Features.Outbox;

namespace ModulebankProject.PipelineBehaviors.Outbox;

public interface ICommandWithEvents
{
    List<OutboxMessage> Events { get; }
}