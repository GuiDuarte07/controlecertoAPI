using FluentValidation;
using ControleCerto.DTOs.Investment;

namespace ControleCerto.Validations
{
    public class DepositInvestmentRequestValidator : AbstractValidator<DepositInvestmentRequest>
    {
        public DepositInvestmentRequestValidator()
        {
            RuleFor(x => x.InvestmentId)
                .GreaterThan(0)
                .WithMessage("ID do investimento é obrigatório e deve ser válido.");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Valor deve ser maior que zero.");

            RuleFor(x => x.AccountId)
                .GreaterThan(0)
                .WithMessage("ID da conta deve ser válido.")
                .When(x => x.AccountId.HasValue);

            RuleFor(x => x.Note)
                .MaximumLength(400)
                .WithMessage("Observação não pode exceder 400 caracteres.")
                .When(x => x.Note != null);

            RuleFor(x => x.OccurredAt)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("Data não pode ser no futuro.")
                .When(x => x.OccurredAt.HasValue);
        }
    }
}
