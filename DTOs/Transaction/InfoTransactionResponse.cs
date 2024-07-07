using System;
using Finantech.DTOs.Account;
using Finantech.DTOs.Category;
using Finantech.DTOs.CreditCard;
using Finantech.DTOs.CreditPurchase;
using Finantech.Enums;

namespace Finantech.Models.DTOs
{
    public class InfoTransactionResponse
    {
        public long Id { get; set; }
        public TransactionTypeEnum Type { get; set; }
        public double Amount { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string Description { get; set; }
        public String? Observations { get; set; }
        public string? Destination { get; set; }
        public bool JustForRecord { get; set; }
        public InfoAccountResponse Account { get; set; }
        public InfoCategoryResponse Category { get; set; }


        // Credit Card Transaction info:
        public int? InstallmentNumber { get; set; }
        public InfoCreditPurchaseResponse? CreditPurchase { get; set; }

    }
}
