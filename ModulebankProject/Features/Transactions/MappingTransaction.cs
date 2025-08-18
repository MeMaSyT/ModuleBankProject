using AutoMapper;

namespace ModulebankProject.Features.Transactions;

public class MappingTransaction : Profile
{
    public MappingTransaction()
    {
        CreateMap<Transaction, TransactionDto>();
    }
}