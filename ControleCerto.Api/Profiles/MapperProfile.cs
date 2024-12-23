using AutoMapper;
using ControleCerto.DTOs.Account;
using ControleCerto.DTOs.Article;
using ControleCerto.DTOs.Category;
using ControleCerto.DTOs.CreditCard;
using ControleCerto.DTOs.CreditPurchase;
using ControleCerto.DTOs.Invoice;
using ControleCerto.DTOs.Notification;
using ControleCerto.DTOs.Transaction;
using ControleCerto.DTOs.TransferenceDTO;
using ControleCerto.DTOs.User;
using ControleCerto.Models.DTOs;
using ControleCerto.Models.Entities;

namespace ControleCerto.Profiles
{
    public class MapperProfile : Profile
    {
        public MapperProfile() 
        {
            CreateMap<User, InfoUserResponse>();
            CreateMap<User, DetailsUserResponse>();

            CreateMap<Account, InfoAccountResponse>();

            CreateMap<CreateAccountRequest, Account>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    dest.CreatedAt = DateTime.UtcNow;
                    dest.UpdatedAt = DateTime.UtcNow;
                });

            CreateMap<Account, InfoAccountResponse>();

            CreateMap<UpdateAccountRequest, Account>()
                .AfterMap((src, dest) =>
                {
                    dest.UpdatedAt = DateTime.UtcNow;
                });


            CreateMap<CreateTransactionRequest, Transaction>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    dest.CreatedAt = DateTime.UtcNow;
                    dest.UpdatedAt = DateTime.UtcNow;
                });
            CreateMap<Transaction, InfoTransactionResponse>();
            CreateMap<UpdateTransactionRequest, Transaction>()
                .AfterMap((src, dest) =>
                {
                    dest.UpdatedAt = DateTime.UtcNow;
                });



            CreateMap<CreateCategoryRequest, Category>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    dest.CreatedAt = DateTime.UtcNow;
                    dest.UpdatedAt = DateTime.UtcNow;
                });
            CreateMap<Category, InfoCategoryResponse>();
            CreateMap<UpdateCategoryRequest, Category>()
                .AfterMap((src, dest) =>
                {
                    dest.UpdatedAt = DateTime.UtcNow;
                });


            CreateMap<CreateCreditCardRequest, CreditCard>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    dest.CreatedAt = DateTime.UtcNow;
                    dest.UpdatedAt = DateTime.UtcNow;
                });
            CreateMap<CreditCard, InfoCreditCardResponse>();        
            CreateMap<UpdateCreditCardRequest, CreditCard>()
                .AfterMap((src, dest) =>
                {
                    dest.UpdatedAt = DateTime.UtcNow;
                });

            CreateMap<CreateCreditPurchaseRequest, CreditPurchase>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    dest.CreatedAt = DateTime.UtcNow;
                    dest.UpdatedAt = DateTime.UtcNow;
                });
            CreateMap<CreditPurchase, InfoCreditPurchaseResponse>();


            CreateMap<Invoice, InfoInvoiceResponse>();

            CreateMap<CreteInvoicePaymentRequest, InvoicePayment>();
            CreateMap<InvoicePayment, InfoInvoicePaymentResponse>();

            CreateMap<CreateTransferenceRequest, Transference>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    dest.CreatedAt = DateTime.UtcNow;
                    dest.UpdatedAt = DateTime.UtcNow;
                });

            CreateMap<CreateNotificationRequest, Notification>();
            CreateMap<Notification, InfoNotificationResponse>();

            CreateMap<Article, InfoArticleResponse>();
        }
    }
}
