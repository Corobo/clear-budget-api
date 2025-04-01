using AutoMapper;
using TransactionsService.Models.DB;
using TransactionsService.Models.DTO;

namespace CategoriesService.Profiles;

public class TransactionProfile : Profile
{
    public TransactionProfile()
    {
        CreateMap<Transaction, TransactionDTO>();
    }
}
