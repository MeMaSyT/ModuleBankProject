using AutoMapper;

namespace ModulebankProject.Features.Accounts
{
    public class MappingAccount : Profile
    {
        public MappingAccount()
        {
            CreateMap<Account, AccountDto>();
            CreateMap<Account, AccountStatementDto>();
        }
    }
}
