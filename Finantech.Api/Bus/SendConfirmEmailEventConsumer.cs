using Finantech.DTOs.Events;
using Finantech.Services.Interfaces;
using MassTransit;

namespace Finantech.Bus
{
    public class SendConfirmEmailEventConsumer : IConsumer<ConfirmEmailEvent> 
    {
        private readonly IEmailService _emailService;

        public SendConfirmEmailEventConsumer(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public Task Consume(ConsumeContext<ConfirmEmailEvent> context)
        {
            _emailService.SendConfirmationEmail(context.Message.User);

            return Task.CompletedTask;
        }
    }
}
