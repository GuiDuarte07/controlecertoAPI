using ControleCerto.Services.Interfaces;
using Hangfire;

namespace ControleCerto.CronJobs
{
    public class HangFireJobs : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public HangFireJobs(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Job 1: todo dia às 3h UTC
            RecurringJob.AddOrUpdate(
                "process-recurring-transactions",
                () => ProcessRecurringTransactionsAsync(),
                "0 3 * * *"
            );

            return Task.CompletedTask;
        }

        public async Task ProcessRecurringTransactionsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var recurringService = scope.ServiceProvider.GetRequiredService<IRecurringTransactionService>();

            await recurringService.ProcessRecurringTransactionsAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
