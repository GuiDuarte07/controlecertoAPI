using ControleCerto.Services.Interfaces;
using Hangfire;

namespace ControleCerto.CronJobs
{
    public class HangFireJobs : IHostedService
    {
        private readonly IRecurringTransactionService _recurringTransactionService;

        public HangFireJobs(IRecurringTransactionService recurringTransactionService)
        {
            _recurringTransactionService = recurringTransactionService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Job 1: todo dia às 3h UTC
            RecurringJob.AddOrUpdate(
                "process-recurring-transactions",
                () => _recurringTransactionService.ProcessRecurringTransactionsAsync(),
                "0 3 * * *"
            );

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
