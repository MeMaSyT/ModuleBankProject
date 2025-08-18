using MediatR;
using ModulebankProject.Infrastructure.Data.Repositories;
using ModulebankProject.MbResult;

namespace ModulebankProject.Features.Accounts.CheckAccountAvailability
{
    // ReSharper disable once UnusedMember.Global используется медиатором, решарпер слишком глуп, чтобы это понять
    public class CheckAccountAvailabilityRequestHandler : IRequestHandler<CheckAccountAvailabilityRequest, MbResult<bool, ApiError>>
    {
        private readonly IAccountsRepository _accountsRepository;

        // ReSharper disable once ConvertToPrimaryConstructor не хочу первичный конструктор
        public CheckAccountAvailabilityRequestHandler(IAccountsRepository accountsRepository)
        {
            _accountsRepository = accountsRepository;
        }
        public async Task<MbResult<bool,ApiError>> Handle(CheckAccountAvailabilityRequest request, CancellationToken cancellationToken)
        {
            var availability = await _accountsRepository.CheckAccountAvailability(request.Id);
            return MbResult<bool, ApiError>.Success(availability);
        }
    }
}
