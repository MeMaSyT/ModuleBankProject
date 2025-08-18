using AutoMapper;
using MediatR;
using ModulebankProject.Infrastructure.Data.Repositories;
using ModulebankProject.MbResult;

namespace ModulebankProject.Features.Transactions.GetTransaction
{
    // ReSharper disable once UnusedMember.Global используется медиатором, решарпер слишком глуп, чтобы это понять
    public class GetTransactionRequestHandler : IRequestHandler<GetTransactionRequest, MbResult<TransactionDto, ApiError>>
    {
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly IMapper _mapper;

        // ReSharper disable once ConvertToPrimaryConstructor не хочу первичный конструктор
        public GetTransactionRequestHandler(IMapper mapper, ITransactionsRepository transactionsRepository)
        {
            _mapper = mapper;
            _transactionsRepository = transactionsRepository;
        }

        public async Task<MbResult<TransactionDto, ApiError>> Handle(GetTransactionRequest request,
            CancellationToken cancellationToken)
        {
            var transaction = await _transactionsRepository.GetTransaction(request.Id);

            if (transaction == null)
                return MbResult<TransactionDto, ApiError>.Failure(new ApiError("Transaction Not Found",
                    StatusCodes.Status404NotFound));

            return MbResult<TransactionDto, ApiError>.Success(_mapper.Map<TransactionDto>(transaction));
        }
    }
}