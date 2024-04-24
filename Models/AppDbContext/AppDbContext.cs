using Finantech.Models.Entities;
using Finantech.Models.MapConfig;
using Microsoft.EntityFrameworkCore;

namespace Finantech.Models.AppDbContext
{
    public class AppDbContext : DbContext
    {

        private readonly IConfiguration _configuration;
        public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration) : base(options) 
        {
            _configuration = configuration;
        }
        public DbSet<User> Users { get; set; }
        public DbSet<CreditCard> CreditCards { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Income> Incomes { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoicePayment> InvoicePayments { get; set; }
        public DbSet<CreditPurchase> CreditPurchases { get; set; }
        public DbSet<CreditExpense> CreditExpenses { get; set; }
        public DbSet<Transference> Transferences { get; set; }
        public DbSet<CategoryDefault> CategoriesDefault { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_configuration.GetConnectionString("WebApiDatabase"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new CreditCardConfiguration());
            modelBuilder.ApplyConfiguration(new AccountConfiguration());
            modelBuilder.ApplyConfiguration(new CategoryConfiguration());
            modelBuilder.ApplyConfiguration(new ExpenseConfiguration());
            modelBuilder.ApplyConfiguration(new IncomeConfiguration());
            modelBuilder.ApplyConfiguration(new InvoiceConfiguration());
            modelBuilder.ApplyConfiguration(new InvoicePaymentConfiguration());
            modelBuilder.ApplyConfiguration(new CreditPurchaseConfiguration());
            modelBuilder.ApplyConfiguration(new CreditExpenseConfiguration());
            modelBuilder.ApplyConfiguration(new TransferenceConfiguration());
            modelBuilder.ApplyConfiguration(new CategoryDefaultConfiguration());
        }

    }
}
