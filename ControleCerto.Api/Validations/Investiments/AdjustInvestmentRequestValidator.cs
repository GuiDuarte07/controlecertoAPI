using FluentValidation;
using ControleCerto.DTOs.Investment;

namespace ControleCerto.Validations
{
    public class AdjustInvestmentRequestValidator : AbstractValidator<AdjustInvestmentRequest>
    {
        public AdjustInvestmentRequestValidator()
        {
            RuleFor(x => x.InvestmentId)
                .GreaterThan(0)
                .WithMessage("ID do investimento é obrigatório e deve ser válido.");

            RuleFor(x => x.NewTotalValue)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Novo valor total não pode ser negativo.");

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
