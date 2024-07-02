using System;
using Finantech.DTOs.Category;
using Finantech.DTOs.Expense;
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
        public string Source { get; set; }
        public InfoCategoryResponse Category { get; set; }
        public TransactionTypeEnum TransactionType { get; set; }
        public int AccountId { get; set; }

        // Construtor para Expense
        public InfoTransactionResponse(Expense expense)
        {
            Id = expense.Id;
            Amount = expense.Amount;
            Description = expense.Description ?? null;
            PurchaseDate = expense.PurchaseDate;
            Source = expense.Destination;
            TransactionType = TransactionTypeEnum.EXPENSE;
            AccountId = expense.AccountId;

            Category = new InfoCategoryResponse
            {
                BillType = expense.Category!.BillType,
                Icon = expense.Category.Icon,
                Id = expense.Category.Id,
                Name = expense.Category.Name,
                Color = expense.Category.Color
            };
        }

        // Construtor para Income
        public InfoTransactionResponse(Income income)
        {
            Id = income.Id;
            Amount = income.Amount;
            Description = income.Description ?? null;
            PurchaseDate = income.PurchaseDate;
            Source = income.Origin;
            TransactionType = TransactionTypeEnum.INCOME;
            AccountId = income.AccountId;

            Category = new InfoCategoryResponse
            {
                BillType = income.Category!.BillType,
                Icon = income.Category.Icon,
                Id = income.Category.Id,
                Name = income.Category.Name,
                Color = income.Category.Color,
            };
        }

        // Construtor para CreditExpense
        public InfoTransactionResponse(CreditExpense creditExpense)
        {
            Id = creditExpense.Id;
            Amount = creditExpense.Amount;
            Description = creditExpense.Description;
            PurchaseDate = creditExpense.PurchaseDate;
            Source = creditExpense.Destination;

            TransactionType = TransactionTypeEnum.CREDITEXPENSE;
            AccountId = creditExpense.AccountId;

            Category = new InfoCategoryResponse
            {
                BillType = creditExpense.Category!.BillType,
                Icon = creditExpense.Category.Icon,
                Id = creditExpense.Category.Id,
                Name = creditExpense.Category.Name,
                Color = creditExpense.Category.Color,
            };
        }
    }
}
