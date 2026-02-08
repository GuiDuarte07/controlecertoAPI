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
using ControleCerto.DTOs.RecurringTransaction;
using ControleCerto.DTOs.Investment;
using ControleCerto.DTOs.Note;
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

            // RecurringTransaction Mappings
            CreateMap<CreateRecurringTransactionRequest, RecurringTransaction>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.RecurrenceRuleId, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Account, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.RecurrenceRule, opt => opt.Ignore())
                .ForMember(dest => dest.Instances, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    dest.CreatedAt = DateTime.UtcNow;
                    dest.UpdatedAt = DateTime.UtcNow;
                });

            CreateMap<UpdateRecurringTransactionRequest, RecurringTransaction>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.RecurrenceRuleId, opt => opt.Ignore())
                .ForMember(dest => dest.Account, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.RecurrenceRule, opt => opt.Ignore())
                .ForMember(dest => dest.Instances, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    dest.UpdatedAt = DateTime.UtcNow;
                });

            CreateMap<RecurringTransaction, InfoRecurringTransactionResponse>()
                .ForMember(dest => dest.PendingInstancesCount, opt => opt.MapFrom(src => 
                    src.Instances.Count(i => i.Status == ControleCerto.Enums.InstanceStatusEnum.PENDING)))
                .ForMember(dest => dest.ConfirmedInstancesCount, opt => opt.MapFrom(src => 
                    src.Instances.Count(i => i.Status == ControleCerto.Enums.InstanceStatusEnum.CONFIRMED)))
                .ForMember(dest => dest.RejectedInstancesCount, opt => opt.MapFrom(src => 
                    src.Instances.Count(i => i.Status == ControleCerto.Enums.InstanceStatusEnum.REJECTED)));

            // RecurrenceRule Mappings
            CreateMap<CreateRecurrenceRuleRequest, RecurrenceRule>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.RecurringTransactions, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    dest.CreatedAt = DateTime.UtcNow;
                });

            CreateMap<RecurrenceRule, InfoRecurrenceRuleResponse>();

            // RecurringTransactionInstance Mappings
            CreateMap<RecurringTransactionInstance, InfoRecurringTransactionInstanceResponse>()
                .ForMember(dest => dest.RecurringTransactionDescription, opt => opt.MapFrom(src => src.RecurringTransaction.Description))
                .ForMember(dest => dest.RecurringTransactionAmount, opt => opt.MapFrom(src => src.RecurringTransaction.Amount))
                .ForMember(dest => dest.RecurringTransactionType, opt => opt.MapFrom(src => src.RecurringTransaction.Type));

            // Investment mappings
            CreateMap<CreateInvestmentRequest, Investment>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    dest.CreatedAt = DateTime.UtcNow;
                    dest.UpdatedAt = DateTime.UtcNow;
                });

            CreateMap<Investment, InfoInvestmentResponse>();
            CreateMap<InvestmentHistory, InvestmentHistoryResponse>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.SourceAccount, opt => opt.MapFrom(src => src.SourceAccount));

            // Note mappings
            CreateMap<CreateNoteRequest, Note>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    dest.CreatedAt = DateTime.UtcNow;
                    dest.UpdatedAt = DateTime.UtcNow;
                });
            CreateMap<Note, NoteResponse>();
        }
    }
}
