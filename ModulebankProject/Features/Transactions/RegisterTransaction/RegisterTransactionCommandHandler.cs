using AutoMapper;
using MediatR;
using ModulebankProject.Infrastructure.Data.Repositories;
using ModulebankProject.MbResult;

namespace ModulebankProject.Features.Transactions.RegisterTransaction
{
    // ReSharper disable once UnusedMember.Global используется медиатором, решарпер слишком глуп, чтобы это понять
    public class RegisterTransactionCommandHandler : IRequestHandler<RegisterTransactionCommand, MbResult<TransactionDto, ApiError>>
    {
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly IMapper _mapper;

        // ReSharper disable once ConvertToPrimaryConstructor не хочу первичный конструктор
        public RegisterTransactionCommandHandler(IMapper mapper, ITransactionsRepository transactionsRepository)
        {
            _mapper = mapper;
            _transactionsRepository = transactionsRepository;
        }
        public async Task<MbResult<TransactionDto, ApiError>> Handle(RegisterTransactionCommand request, CancellationToken cancellationToken)
        {
            var transaction = await _transactionsRepository.RegisterTransaction(request);
            if(!transaction.IsSuccess) return MbResult<TransactionDto, ApiError>.Failure(transaction.Error!);

            return MbResult<TransactionDto, ApiError>.Success(_mapper.Map<TransactionDto>(transaction.Result));
        }
    }
}
