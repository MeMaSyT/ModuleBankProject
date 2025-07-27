using MediatR;

namespace ModulebankProject.Features.Accounts.GetAccountStatement
{
    public record GetAccountStatementRequest(Guid Id, DateTime StartRangeDate, DateTime EndRangeDate) : IRequest<AccountStatementDto?>;
}
