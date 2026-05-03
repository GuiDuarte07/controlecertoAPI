using ControleCerto.Modules.Tickets.DTOs;
using FluentValidation;

namespace ControleCerto.Validations.Tickets
{
    public class UpdateTicketUserRequestValidator : AbstractValidator<UpdateTicketUserRequest>
    {
        public UpdateTicketUserRequestValidator()
        {
            RuleFor(x => x.Action)
                .NotEmpty()
                .Must(action =>
                {
                    var value = (action ?? string.Empty).Trim().ToLowerInvariant();
                    return value == "close" || value == "reopen";
                })
                .WithMessage("Ação inválida.");
        }
    }
}

