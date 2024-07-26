using Finantech.DTOs.Events;
using Finantech.Services.Interfaces;
using MassTransit;

namespace Finantech.Bus
{
    public class SendForgotPasswordEventConsumer: IConsumer<ForgotPasswordEvent>
    {
        private readonly IEmailService _emailService;

        public SendForgotPasswordEventConsumer(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public Task Consume(ConsumeContext<ForgotPasswordEvent> context)
        {
            var email = context.Message.Email;

            _emailService.SendForgotPasswordEmail(email);

            return Task.CompletedTask;      
        }
    }
}
