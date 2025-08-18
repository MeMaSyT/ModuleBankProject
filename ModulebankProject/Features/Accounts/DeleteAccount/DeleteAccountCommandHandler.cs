using MediatR;
using ModulebankProject.Infrastructure.Data.Repositories;
using ModulebankProject.MbResult;

namespace ModulebankProject.Features.Accounts.DeleteAccount;

// ReSharper disable once UnusedMember.Global используется медиатором, решарпер слишком глуп, чтобы это понять
public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, MbResult<Guid, ApiError>>
{
    private readonly IAccountsRepository _accountsRepository;

    // ReSharper disable once ConvertToPrimaryConstructor не хочу первичный конструктор
    public DeleteAccountCommandHandler(IAccountsRepository accountsRepository)
    {
        _accountsRepository = accountsRepository;
    }
    public async Task<MbResult<Guid, ApiError>> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var deleteId = await _accountsRepository.DeleteAccount(request.Id);
        if (!deleteId.IsSuccess) return MbResult<Guid, ApiError>.Failure(deleteId.Error!);

        return deleteId.Result == Guid.Empty ? MbResult<Guid, ApiError>.Failure(new ApiError("Deleting Account Not Found", StatusCodes.Status404NotFound)) : MbResult<Guid, ApiError>.Success(deleteId.Result);
    }
}