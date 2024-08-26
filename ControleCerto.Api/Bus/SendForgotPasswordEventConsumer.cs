using ControleCerto.DTOs.Events;
using ControleCerto.Services.Interfaces;
using MassTransit;

namespace ControleCerto.Bus
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
