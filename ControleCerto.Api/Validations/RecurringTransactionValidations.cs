using ControleCerto.Enums;
using ControleCerto.Models.Entities;
using ControleCerto.Errors;
using ControleCerto.DTOs.RecurringTransaction;

namespace ControleCerto.Validations
{
    public static class RecurringTransactionValidations
    {
        public static Result<bool> ValidateCreateRecurringTransactionRequest(CreateRecurringTransactionRequest request)
        {
            var ruleValidation = ValidateRecurrenceRuleRequest(request.RecurrenceRule);
            if (!ruleValidation.IsSuccess)
                return ruleValidation;

            if (request.Type != TransactionTypeEnum.EXPENSE && 
                request.Type != TransactionTypeEnum.INCOME)
            {
                return new AppError("Transações recorrentes só podem ser do tipo EXPENSE ou INCOME.", ErrorTypeEnum.BusinessRule);
            }

            if (request.Amount <= 0)
            {
                return new AppError("Valor deve ser maior que zero.", ErrorTypeEnum.BusinessRule);
            }

            if (request.StartDate < DateTime.UtcNow.Date)
            {
                return new AppError("Data de início não pode ser no passado.", ErrorTypeEnum.BusinessRule);
            }

            if (request.EndDate.HasValue && 
                request.EndDate.Value <= request.StartDate)
            {
                return new AppError("Data de fim deve ser posterior à data de início.", ErrorTypeEnum.BusinessRule);
            }

            if (string.IsNullOrWhiteSpace(request.Description))
            {
                return new AppError("Descrição é obrigatória.", ErrorTypeEnum.BusinessRule);
            }

            return true;
        }

        public static Result<bool> ValidateRecurrenceRuleRequest(CreateRecurrenceRuleRequest request)
        {
            if (!Enum.IsDefined(typeof(RecurrenceFrequencyEnum), request.Frequency))
            {
                return new AppError("Frequência de recorrência inválida.", ErrorTypeEnum.BusinessRule);
            }

            if (request.Interval <= 0)
            {
                return new AppError("Intervalo deve ser maior que zero.", ErrorTypeEnum.BusinessRule);
            }

            return request.Frequency switch
            {
                RecurrenceFrequencyEnum.DAILY => ValidateDailyRule(request),
                RecurrenceFrequencyEnum.WEEKLY => ValidateWeeklyRule(request),
                RecurrenceFrequencyEnum.MONTHLY => ValidateMonthlyRule(request),
                RecurrenceFrequencyEnum.YEARLY => ValidateYearlyRule(request),
                _ => new AppError("Frequência não suportada.", ErrorTypeEnum.BusinessRule)
            };
        }

        private static Result<bool> ValidateDailyRule(CreateRecurrenceRuleRequest request)
        {
            if (!request.IsEveryDay && string.IsNullOrEmpty(request.DaysOfWeek))
            {
                return new AppError("Para recorrência diária, deve especificar se é todos os dias ou os dias da semana.", ErrorTypeEnum.BusinessRule);
            }

            if (!request.IsEveryDay && !IsValidDaysOfWeek(request.DaysOfWeek!))
            {
                return new AppError("Formato inválido para dias da semana. Use formato de sequencia de números para cada dia: 0123456", ErrorTypeEnum.BusinessRule);
            }

            return true;
        }

        private static Result<bool> ValidateWeeklyRule(CreateRecurrenceRuleRequest request)
        {
            if (!request.DayOfWeek.HasValue || request.DayOfWeek < 0 || request.DayOfWeek > 6)
            {
                return new AppError("Para recorrência semanal, deve especificar um dia da semana válido (0-6).", ErrorTypeEnum.BusinessRule);
            }

            return true;
        }

        private static Result<bool> ValidateMonthlyRule(CreateRecurrenceRuleRequest request)
        {
            if (!request.DayOfMonth.HasValue)
            {
                return new AppError("Para recorrência mensal, deve especificar o dia do mês.", ErrorTypeEnum.BusinessRule);
            }

            if (request.DayOfMonth < -1 || request.DayOfMonth == 0 || request.DayOfMonth > 28)
            {
                return new AppError("Dia do mês deve estar entre 1-28 ou -1 para último dia do mês.", ErrorTypeEnum.BusinessRule);
            }

            return true;
        }

        private static Result<bool> ValidateYearlyRule(CreateRecurrenceRuleRequest request)
        {
            if (!request.MonthOfYear.HasValue || request.MonthOfYear < 1 || request.MonthOfYear > 12)
            {
                return new AppError("Para recorrência anual, deve especificar um mês válido (1-12).", ErrorTypeEnum.BusinessRule);
            }

            if (!request.DayOfMonthForYearly.HasValue || request.DayOfMonthForYearly < 1 || request.DayOfMonthForYearly > 31)
            {
                return new AppError("Para recorrência anual, deve especificar um dia válido (1-31).", ErrorTypeEnum.BusinessRule);
            }

            if (!IsValidDate(request.DayOfMonthForYearly.Value, request.MonthOfYear.Value))
            {
                return new AppError($"Data inválida: {request.DayOfMonthForYearly}/{request.MonthOfYear}.", ErrorTypeEnum.BusinessRule);
            }

            return true;
        }

        private static bool IsValidDaysOfWeek(string daysOfWeek)
        {
            if (string.IsNullOrEmpty(daysOfWeek)) return false;
            if (daysOfWeek.Length > 7) return false;

            // Valida dígito válido entre 0 e 6
            foreach (char c in daysOfWeek)
            {
                if (!char.IsDigit(c)) return false;
                int day = c - '0';
                if (day < 0 || day > 6) return false;
            }

            // Valida números repetidos
            if (daysOfWeek.Distinct().Count() != daysOfWeek.Length) return false;

            return true;
        }


        private static bool IsValidDate(int day, int month)
        {
            try
            {
                new DateTime(2024, month, day); // Usar ano bissexto para testar 29/02
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
