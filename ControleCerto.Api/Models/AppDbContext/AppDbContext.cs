using ControleCerto.Models.Entities;
using ControleCerto.Models.MapConfig;
using Microsoft.EntityFrameworkCore;

namespace ControleCerto.Models.AppDbContext
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
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoicePayment> InvoicePayments { get; set; }
        public DbSet<CreditPurchase> CreditPurchases { get; set; }
        public DbSet<Transference> Transferences { get; set; }
        public DbSet<CategoryDefault> CategoriesDefault { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<CategoryLimit> CategoryLimits { get; set; }
        public DbSet<RecurrenceRule> RecurrenceRules { get; set; }
        public DbSet<RecurringTransaction> RecurringTransactions { get; set; }
        public DbSet<RecurringTransactionInstance> RecurringTransactionInstances { get; set; }

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
            modelBuilder.ApplyConfiguration(new TransactionConfiguration());
            modelBuilder.ApplyConfiguration(new InvoiceConfiguration());
            modelBuilder.ApplyConfiguration(new InvoicePaymentConfiguration());
            modelBuilder.ApplyConfiguration(new CreditPurchaseConfiguration());
            modelBuilder.ApplyConfiguration(new TransferenceConfiguration());
            modelBuilder.ApplyConfiguration(new CategoryDefaultConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationConfiguration());
            modelBuilder.ApplyConfiguration(new ArticleConfiguration());
            modelBuilder.ApplyConfiguration(new CategoryLimitConfiguration());
            modelBuilder.ApplyConfiguration(new RecurrenceRuleConfiguration());
            modelBuilder.ApplyConfiguration(new RecurringTransactionConfiguration());
            modelBuilder.ApplyConfiguration(new RecurringTransactionInstanceConfiguration());
        }

    }
}
