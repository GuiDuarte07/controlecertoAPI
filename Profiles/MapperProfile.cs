﻿using AutoMapper;
using Finantech.DTOs.Account;
using Finantech.DTOs.Category;
using Finantech.DTOs.CreditCard;
using Finantech.DTOs.CreditPurchase;
using Finantech.DTOs.Invoice;
using Finantech.DTOs.Transaction;
using Finantech.DTOs.TransferenceDTO;
using Finantech.DTOs.User;
using Finantech.Models.DTOs;
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

            CreateMap<UpdateAccountRequest, Account>()
                .AfterMap((src, dest) =>
                {
                    dest.UpdatedAt = DateTime.Now;
                });


            CreateMap<CreateTransactionRequest, Transaction>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    dest.CreatedAt = DateTime.Now;
                    dest.UpdatedAt = DateTime.Now;
                });
            CreateMap<Transaction, InfoTransactionResponse>();
            CreateMap<UpdateTransactionRequest, Transaction>()
                .AfterMap((src, dest) =>
                {
                    dest.UpdatedAt = DateTime.Now;
                });



            CreateMap<CreateCategoryRequest, Category>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    dest.CreatedAt = DateTime.Now;
                    dest.UpdatedAt = DateTime.Now;
                });
            CreateMap<Category, InfoCategoryResponse>();
            CreateMap<UpdateCategoryRequest, Category>()
                .AfterMap((src, dest) =>
                {
                    dest.UpdatedAt = DateTime.Now;
                });


            CreateMap<CreateCreditCardRequest, CreditCard>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    dest.CreatedAt = DateTime.Now;
                    dest.UpdatedAt = DateTime.Now;
                });
            CreateMap<CreditCard, InfoCreditCardResponse>();        
            CreateMap<UpdateCreditCardRequest, CreditCard>()
                .AfterMap((src, dest) =>
                {
                    dest.UpdatedAt = DateTime.Now;
                });

            CreateMap<CreateCreditPurchaseRequest, CreditPurchase>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    dest.CreatedAt = DateTime.Now;
                    dest.UpdatedAt = DateTime.Now;
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
                    dest.CreatedAt = DateTime.Now;
                    dest.UpdatedAt = DateTime.Now;
                });
        }
    }
}
