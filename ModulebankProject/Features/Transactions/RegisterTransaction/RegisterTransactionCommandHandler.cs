using AutoMapper;
using MediatR;
using ModulebankProject.Infrastructure.Data.Repositories;

namespace ModulebankProject.Features.Transactions.RegisterTransaction
{
    public class RegisterTransactionCommandHandler : IRequestHandler<RegisterTransactionCommand, TransactionDto>
    {
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly IMapper _mapper;

        public RegisterTransactionCommandHandler(IMapper mapper, ITransactionsRepository transactionsRepository)
        {
            _mapper = mapper;
            _transactionsRepository = transactionsRepository;
        }
        public async Task<TransactionDto> Handle(RegisterTransactionCommand request, CancellationToken cancellationToken)
        {
            return _mapper.Map<TransactionDto>(await _transactionsRepository.RegisterTransaction(request));
        }
    }
}
