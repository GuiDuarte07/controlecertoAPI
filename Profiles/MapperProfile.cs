using AutoMapper;
using Finantech.DTOs.Account;
using Finantech.Models.Entities;

namespace Finantech.Profiles
{
    public class MapperProfile : Profile
    {
        public MapperProfile() 
        {
            CreateMap<CreateAccountRequest, Account>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    dest.CreatedAt = DateTime.Now;
                    dest.UpdatedAt = DateTime.Now;
                });
            CreateMap<Account, InfoAccountResponse>();
        }
    }
}
