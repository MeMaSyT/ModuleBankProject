using MediatR;

namespace ModulebankProject.Features.Accounts.CheckAccountAvailability
{
    public record CheckAccountAvailabilityRequest(Guid Id) : IRequest<bool>;
}
