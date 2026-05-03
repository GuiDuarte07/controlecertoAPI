using ControleCerto.Modules.Tickets.DTOs;
using FluentValidation;

namespace ControleCerto.Validations.Tickets
{
    public class UpdateTicketAdminRequestValidator : AbstractValidator<UpdateTicketAdminRequest>
    {
        public UpdateTicketAdminRequestValidator()
        {
            RuleFor(x => x)
                .Must(x => x.Status.HasValue || x.Priority.HasValue)
                .WithMessage("Informe ao menos um campo para atualização.");
        }
    }
}

