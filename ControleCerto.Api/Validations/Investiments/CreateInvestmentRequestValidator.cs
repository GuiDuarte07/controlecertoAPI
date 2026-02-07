using FluentValidation;
using ControleCerto.DTOs.Investment;

namespace ControleCerto.Validations
{
    public class CreateInvestmentRequestValidator : AbstractValidator<CreateInvestmentRequest>
    {
        public CreateInvestmentRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Nome do investimento é obrigatório.")
                .Length(3, 120)
                .WithMessage("Nome deve ter entre 3 e 120 caracteres.");

            RuleFor(x => x.Description)
                .MaximumLength(300)
                .WithMessage("Descrição não pode exceder 300 caracteres.")
                .When(x => x.Description != null);

            RuleFor(x => x.InitialAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Valor inicial não pode ser negativo.")
                .When(x => x.InitialAmount.HasValue);
        }
    }
}
