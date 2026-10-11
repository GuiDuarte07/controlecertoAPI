using ControleCerto.Modules.InvestmentAnalytics.DTOs;
using FluentValidation;

namespace ControleCerto.Modules.InvestmentAnalytics.Validations;

public sealed class InvestmentAnalyticsRequestValidator : AbstractValidator<InvestmentAnalyticsRequest>
{
    public InvestmentAnalyticsRequestValidator()
    {
        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(new DateTime(1900, 1, 1))
            .WithMessage("Informe uma data inicial válida, a partir de 1900.");
        RuleFor(x => x.EndDate)
            .LessThan(new DateTime(9999, 1, 1))
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("A data final deve ser válida e igual ou posterior à data inicial.");
        RuleFor(x => x.EndDate)
            .LessThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("A data final não pode estar no futuro.");
        RuleFor(x => x)
            .Must(x => (x.EndDate.Date - x.StartDate.Date).TotalDays <= 3653)
            .WithMessage("Consulte até dez anos por vez.");
        RuleFor(x => x.InvestmentId)
            .GreaterThan(0)
            .When(x => x.InvestmentId.HasValue)
            .WithMessage("Selecione um investimento válido.");
    }
}
