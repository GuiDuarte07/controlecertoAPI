using Finantech.Enums;

namespace Finantech.Models.Entities
{
    public class Account
    {
        public int Id { get; set; }
        public double Balance { get; set; }
        public string Description { get; set; }
        public string Bank { get; set; }
        public AccountTypeEnum AccountType { get; set; }
        public string Color { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public User User { get; set; }
        public CreditCard? CreditCard { get; set; }
        public ICollection<Expense> Expenses { get; set; }
        public ICollection<Income> Incomes { get; set; }
        public ICollection<CreditExpense> CreditExpenses { get; set; }
        public ICollection<Transference> Transferences { get; set; }
    }
}
