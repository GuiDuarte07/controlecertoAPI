using FluentValidation;
using ControleCerto.DTOs.Investment;

namespace ControleCerto.Validations
{
    public class UpdateInvestmentRequestValidator : AbstractValidator<UpdateInvestmentRequest>
    {
        public UpdateInvestmentRequestValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("ID do investimento é obrigatório e deve ser válido.");

            RuleFor(x => x.Name)
                .Length(3, 120)
                .WithMessage("Nome deve ter entre 3 e 120 caracteres.")
                .When(x => x.Name != null);

            RuleFor(x => x.Description)
                .MaximumLength(300)
                .WithMessage("Descrição não pode exceder 300 caracteres.")
                .When(x => x.Description != null);
        }
    }
}
