using Finantech.DTOs.Events;
using Finantech.Services.Interfaces;
using MassTransit;

namespace Finantech.Bus
{
    public class SendEmailEventConsumer: IConsumer<EmailEvent>
    {
        private readonly IEmailService _emailService;

        public SendEmailEventConsumer(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task Consume(ConsumeContext<EmailEvent> context)
        {
            var email = context.Message;

            _emailService.SendEmail(email.Emails, email.Subject, email.Body);

            await Task.Delay(1);
        }
    }
}
