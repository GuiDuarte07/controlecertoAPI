using ControleCerto.Modules.TransactionAnalytics.DTOs;
using FluentValidation;

namespace ControleCerto.Modules.TransactionAnalytics.Validations;

public sealed class TransactionAnalyticsRequestValidator : AbstractValidator<TransactionAnalyticsRequest>
{
    public TransactionAnalyticsRequestValidator()
    {
        RuleFor(x => x.StartDate).GreaterThanOrEqualTo(new DateTime(1900, 1, 1))
            .WithMessage("Informe uma data inicial válida, a partir de 1900.");
        RuleFor(x => x.EndDate).LessThan(new DateTime(9999, 1, 1))
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("A data final deve ser válida e igual ou posterior à data inicial.");
        RuleFor(x => x.EndDate).LessThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("A data final não pode estar no futuro.");
        RuleFor(x => x).Must(x => (x.EndDate.Date - x.StartDate.Date).TotalDays <= 1826)
            .WithMessage("Consulte até cinco anos por vez.");
        RuleFor(x => x.CreditDateBasis).Must(x => x is "invoice" or "transaction")
            .WithMessage("Selecione mês da fatura ou data do lançamento para as despesas no cartão.");
    }
}
