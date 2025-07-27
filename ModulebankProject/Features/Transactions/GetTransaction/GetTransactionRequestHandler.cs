using AutoMapper;
using MediatR;
using ModulebankProject.Infrastructure.Data.Repositories;

namespace ModulebankProject.Features.Transactions.GetTransaction
{
    public class GetTransactionRequestHandler : IRequestHandler<GetTransactionRequest, TransactionDto?>
    {
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly IMapper _mapper;

        public GetTransactionRequestHandler(IMapper mapper, ITransactionsRepository transactionsRepository)
        {
            _mapper = mapper;
            _transactionsRepository = transactionsRepository;
        }
        public async Task<TransactionDto?> Handle(GetTransactionRequest request, CancellationToken cancellationToken)
        {
            Transaction? transaction = await _transactionsRepository.GetTransaction(request.Id);

            if (transaction != null)
            {
                return _mapper.Map<TransactionDto>(transaction);
            }

            return null;
        }
    }
}
