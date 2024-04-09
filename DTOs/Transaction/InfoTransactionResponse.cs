using System;
using Finantech.Enums;
using Finantech.Models.Entities;

namespace Finantech.Models.DTOs
{
    public class InfoTransactionResponse
    {
        public int Id { get; set; }
        public double Amount { get; set; }
        public string? Description { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string? Origin { get; set; }
        public string? Destination { get; set; }
        public Category Category { get; set; }
        public TransactionTypeEnum TransactionType { get; set; }

        // Construtor para Expense
        public InfoTransactionResponse(Expense expense)
        {
            Id = expense.Id;
            Amount = expense.Amount;
            Description = expense.Description ?? null;
            PurchaseDate = expense.PurchaseDate;
            Destination = expense.Destination;
            Category = expense.Category;
            TransactionType = TransactionTypeEnum.EXPENSE;
        }

        // Construtor para Income
        public InfoTransactionResponse(Income income)
        {
            Id = income.Id;
            Amount = income.Amount;
            Description = income.Description ?? null;
            PurchaseDate = income.PurchaseDate;
            Origin = income.Origin;
            Category = income.Category;
            TransactionType = TransactionTypeEnum.INCOME;
        }

        // Construtor para CreditExpense
        public InfoTransactionResponse(CreditExpense creditExpense)
        {
            Id = creditExpense.Id;
            Amount = creditExpense.Amount;
            Description = creditExpense.Description;
            PurchaseDate = creditExpense.PurchaseDate;
            Destination = creditExpense.Destination;
            Category = creditExpense.Category;
            TransactionType = TransactionTypeEnum.CREDITEXPENSE;
        }
    }
}
