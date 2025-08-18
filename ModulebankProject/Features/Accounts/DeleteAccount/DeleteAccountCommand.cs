using MediatR;
using ModulebankProject.MbResult;

namespace ModulebankProject.Features.Accounts.DeleteAccount;

/// <summary>
/// Command для удаления счёта
/// </summary>
/// <param name="Id">номер счёта</param>
public record DeleteAccountCommand(Guid Id) : IRequest<MbResult<Guid, ApiError>>;