using MediatR;
using ModulebankProject.MbResult;

namespace ModulebankProject.Features.Accounts.CheckAccountAvailability
{
    /// <summary>
    /// Request для проверки существования счёта
    /// </summary>
    /// <param name="Id">Номер счёта</param>
    public record CheckAccountAvailabilityRequest(Guid Id) : IRequest<MbResult<bool, ApiError>>;
}
