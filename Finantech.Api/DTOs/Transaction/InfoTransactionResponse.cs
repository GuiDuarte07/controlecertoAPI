using System;
using Finantech.DTOs.Account;
using Finantech.DTOs.Category;
using Finantech.DTOs.CreditCard;
using Finantech.DTOs.CreditPurchase;
using Finantech.Enums;
using Finantech.Models.Entities;

namespace Finantech.Models.DTOs
{
    public class InfoTransactionResponse
    {
        public long Id { get; set; }
        public TransactionTypeEnum Type { get; set; }
        public double Amount { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string Description { get; set; }
        public string? Observations { get; set; }
        public string? Destination { get; set; }
        public bool JustForRecord { get; set; }
        public InfoAccountResponse Account { get; set; }
        public InfoCategoryResponse Category { get; set; }


        // Credit Card Transaction info:
        public int? InstallmentNumber { get; set; }
        public InfoCreditPurchaseResponse? CreditPurchase { get; set; }

        public InfoTransactionResponse()
        {
        }

        public InfoTransactionResponse(InvoicePayment invoicePayment)
        {
            Id = invoicePayment.Id;
            Type = TransactionTypeEnum.INVOICEPAYMENT;
            Amount = invoicePayment.AmountPaid;
            PurchaseDate = invoicePayment.PaymentDate;
            Description = invoicePayment.Description;
            Destination = "Pagamento Fatura";
            JustForRecord = invoicePayment.JustForRecord;
            Account = new InfoAccountResponse
            {
                Balance = invoicePayment.Account.Balance,
                Description = invoicePayment.Account.Description,
                Id = invoicePayment.Account.Id,
                Bank = invoicePayment.Account.Bank,
                Color = invoicePayment.Account.Color
            };
        }

    }
}
