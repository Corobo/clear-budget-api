using AutoMapper;
using TransactionsService.Models.DB;
using TransactionsService.Models.DTO;

namespace TransactionsService.Profiles
{
    public class TransactionProfile : Profile
    {
        public TransactionProfile()
        {
            CreateMap<Transaction, TransactionDTO>().ReverseMap();
            CreateMap<CreateTransactionDTO, Transaction>();
            CreateMap<UpdateTransactionDTO, Transaction>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
