using ControleCerto.Enums;
using ControleCerto.Errors;
using ControleCerto.DTOs.RecurringTransaction;
using ControleCerto.Validations;
using FluentAssertions;
using Xunit;

namespace ControleCerto.Tests.Validations
{
    public class RecurringTransactionValidationsTests
    {
        #region ValidateRecurrenceRuleRequest Tests

        [Fact]
        public void ValidateRecurrenceRuleRequest_WithValidDailyRule_ShouldReturnSuccess()
        {
            // Arrange
            var request = new CreateRecurrenceRuleRequest
            {
                Frequency = RecurrenceFrequencyEnum.DAILY,
                IsEveryDay = true,
                Interval = 1
            };

            // Act
            var result = RecurringTransactionValidations.ValidateRecurrenceRuleRequest(request);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void ValidateRecurrenceRuleRequest_WithInvalidFrequency_ShouldReturnError()
        {
            // Arrange
            var request = new CreateRecurrenceRuleRequest
            {
                Frequency = (RecurrenceFrequencyEnum)999, // Valor inválido
                IsEveryDay = true,
                Interval = 1
            };

            // Act
            var result = RecurringTransactionValidations.ValidateRecurrenceRuleRequest(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorMessage.Should().Be("Frequência de recorrência inválida.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-5)]
        public void ValidateRecurrenceRuleRequest_WithInvalidInterval_ShouldReturnError(int interval)
        {
            // Arrange
            var request = new CreateRecurrenceRuleRequest
            {
                Frequency = RecurrenceFrequencyEnum.DAILY,
                IsEveryDay = true,
                Interval = interval
            };

            // Act
            var result = RecurringTransactionValidations.ValidateRecurrenceRuleRequest(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorMessage.Should().Be("Intervalo deve ser maior que zero.");
        }

        #endregion

        #region ValidateDailyRule Tests

        [Fact]
        public void ValidateDailyRule_WithEveryDayTrue_ShouldReturnSuccess()
        {
            // Arrange
            var request = new CreateRecurrenceRuleRequest
            {
                Frequency = RecurrenceFrequencyEnum.DAILY,
                IsEveryDay = true,
                Interval = 1
            };

            // Act
            var result = RecurringTransactionValidations.ValidateRecurrenceRuleRequest(request);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void ValidateDailyRule_WithValidDaysOfWeek_ShouldReturnSuccess()
        {
            // Arrange
            var request = new CreateRecurrenceRuleRequest
            {
                Frequency = RecurrenceFrequencyEnum.DAILY,
                IsEveryDay = false,
                DaysOfWeek = "12345", // Segunda a sexta
                Interval = 1
            };

            // Act
            var result = RecurringTransactionValidations.ValidateRecurrenceRuleRequest(request);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void ValidateDailyRule_WithInvalidDaysOfWeek_ShouldReturnError()
        {
            // Arrange
            var request = new CreateRecurrenceRuleRequest
            {
                Frequency = RecurrenceFrequencyEnum.DAILY,
                IsEveryDay = false,
                DaysOfWeek = "12845", // Dia 8 inválido
                Interval = 1
            };

            // Act
            var result = RecurringTransactionValidations.ValidateRecurrenceRuleRequest(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorMessage.Should().Be("Formato inválido para dias da semana. Use formato de sequencia de números para cada dia: 0123456");
        }

        [Theory]
        [InlineData("12345")] // Segunda a sexta
        [InlineData("135")] // Segunda, quarta, sexta
        [InlineData("0")] // Domingo
        [InlineData("01")] // Domingoe e segunda
        public void ValidateDailyRule_WithValidDaysOfWeekFormats_ShouldReturnSuccess(string daysOfWeek)
        {
            // Arrange
            var request = new CreateRecurrenceRuleRequest
            {
                Frequency = RecurrenceFrequencyEnum.DAILY,
                IsEveryDay = false,
                DaysOfWeek = daysOfWeek,
                Interval = 1
            };

            // Act
            var result = RecurringTransactionValidations.ValidateRecurrenceRuleRequest(request);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        #endregion

        #region ValidateWeeklyRule Tests

        [Theory]
        [InlineData(0)] // Domingo
        [InlineData(1)] // Segunda
        [InlineData(2)] // Terça
        [InlineData(3)] // Quarta
        [InlineData(4)] // Quinta
        [InlineData(5)] // Sexta
        [InlineData(6)] // Sábado
        public void ValidateWeeklyRule_WithValidDayOfWeek_ShouldReturnSuccess(int dayOfWeek)
        {
            // Arrange
            var request = new CreateRecurrenceRuleRequest
            {
                Frequency = RecurrenceFrequencyEnum.WEEKLY,
                DayOfWeek = dayOfWeek,
                Interval = 1
            };

            // Act
            var result = RecurringTransactionValidations.ValidateRecurrenceRuleRequest(request);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Theory]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(-1)]
        [InlineData(15)]
        public void ValidateWeeklyRule_WithInvalidDayOfWeek_ShouldReturnError(int dayOfWeek)
        {
            // Arrange
            var request = new CreateRecurrenceRuleRequest
            {
                Frequency = RecurrenceFrequencyEnum.WEEKLY,
                DayOfWeek = dayOfWeek,
                Interval = 1
            };

            // Act
            var result = RecurringTransactionValidations.ValidateRecurrenceRuleRequest(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorMessage.Should().Be("Para recorrência semanal, deve especificar um dia da semana válido (0-6).");
        }

        #endregion

        #region ValidateMonthlyRule Tests

        [Theory]
        [InlineData(1)] // Dia 1
        [InlineData(15)] // Dia 15
        [InlineData(28)] // Dia 28
        [InlineData(-1)] // Último dia do mês
        public void ValidateMonthlyRule_WithValidDayOfMonth_ShouldReturnSuccess(int dayOfMonth)
        {
            // Arrange
            var request = new CreateRecurrenceRuleRequest
            {
                Frequency = RecurrenceFrequencyEnum.MONTHLY,
                DayOfMonth = dayOfMonth,
                Interval = 1
            };

            // Act
            var result = RecurringTransactionValidations.ValidateRecurrenceRuleRequest(request);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(29)] // Dia 29 - não existe em fevereiro
        [InlineData(30)] // Dia 30 - não existe em fevereiro
        [InlineData(31)] // Dia 31 - não existe em fevereiro, abril, junho, setembro, novembro
        [InlineData(32)]
        [InlineData(-2)]
        [InlineData(50)]
        public void ValidateMonthlyRule_WithInvalidDayOfMonth_ShouldReturnError(int dayOfMonth)
        {
            // Arrange
            var request = new CreateRecurrenceRuleRequest
            {
                Frequency = RecurrenceFrequencyEnum.MONTHLY,
                DayOfMonth = dayOfMonth,
                Interval = 1
            };

            // Act
            var result = RecurringTransactionValidations.ValidateRecurrenceRuleRequest(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorMessage.Should().Be("Dia do mês deve estar entre 1-28 ou -1 para último dia do mês.");
        }

        #endregion

        #region ValidateYearlyRule Tests

        [Theory]
        [InlineData(1, 1)] // 1º de janeiro
        [InlineData(15, 6)] // 15 de junho
        [InlineData(31, 12)] // 31 de dezembro
        [InlineData(29, 2)] // 29 de fevereiro (ano bissexto)
        public void ValidateYearlyRule_WithValidDate_ShouldReturnSuccess(int day, int month)
        {
            // Arrange
            var request = new CreateRecurrenceRuleRequest
            {
                Frequency = RecurrenceFrequencyEnum.YEARLY,
                MonthOfYear = month,
                DayOfMonthForYearly = day,
                Interval = 1
            };

            // Act
            var result = RecurringTransactionValidations.ValidateRecurrenceRuleRequest(request);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Theory]
        [InlineData(31, 2)] // 31 de fevereiro
        [InlineData(30, 2)] // 30 de fevereiro
        [InlineData(31, 4)] // 31 de abril
        [InlineData(31, 6)] // 31 de junho
        [InlineData(31, 9)] // 31 de setembro
        [InlineData(31, 11)] // 31 de novembro
        public void ValidateYearlyRule_WithInvalidDate_ShouldReturnError(int day, int month)
        {
            // Arrange
            var request = new CreateRecurrenceRuleRequest
            {
                Frequency = RecurrenceFrequencyEnum.YEARLY,
                MonthOfYear = month,
                DayOfMonthForYearly = day,
                Interval = 1
            };

            // Act
            var result = RecurringTransactionValidations.ValidateRecurrenceRuleRequest(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorMessage.Should().Contain("Data inválida:");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(13)]
        [InlineData(-1)]
        public void ValidateYearlyRule_WithInvalidMonth_ShouldReturnError(int month)
        {
            // Arrange
            var request = new CreateRecurrenceRuleRequest
            {
                Frequency = RecurrenceFrequencyEnum.YEARLY,
                MonthOfYear = month,
                DayOfMonthForYearly = 1,
                Interval = 1
            };

            // Act
            var result = RecurringTransactionValidations.ValidateRecurrenceRuleRequest(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorMessage.Should().Be("Para recorrência anual, deve especificar um mês válido (1-12).");
        }

        #endregion

        #region ValidateCreateRecurringTransactionRequest Tests

        [Fact]
        public void ValidateCreateRecurringTransactionRequest_WithValidExpense_ShouldReturnSuccess()
        {
            // Arrange
            var request = new CreateRecurringTransactionRequest
            {
                Type = TransactionTypeEnum.EXPENSE,
                Amount = 100.50,
                Description = "Aluguel",
                StartDate = DateTime.UtcNow.AddDays(1),
                AccountId = 1,
                CategoryId = 1,
                RecurrenceRule = new CreateRecurrenceRuleRequest
                {
                    Frequency = RecurrenceFrequencyEnum.MONTHLY,
                    DayOfMonth = 15,
                    Interval = 1
                }
            };

            // Act
            var result = RecurringTransactionValidations.ValidateCreateRecurringTransactionRequest(request);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void ValidateCreateRecurringTransactionRequest_WithValidIncome_ShouldReturnSuccess()
        {
            // Arrange
            var request = new CreateRecurringTransactionRequest
            {
                Type = TransactionTypeEnum.INCOME,
                Amount = 5000.00,
                Description = "Salário",
                StartDate = DateTime.UtcNow.AddDays(1),
                AccountId = 1,
                CategoryId = 1,
                RecurrenceRule = new CreateRecurrenceRuleRequest
                {
                    Frequency = RecurrenceFrequencyEnum.MONTHLY,
                    DayOfMonth = 5,
                    Interval = 1
                }
            };

            // Act
            var result = RecurringTransactionValidations.ValidateCreateRecurringTransactionRequest(request);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void ValidateCreateRecurringTransactionRequest_WithInvalidType_ShouldReturnError()
        {
            // Arrange
            var request = new CreateRecurringTransactionRequest
            {
                Type = TransactionTypeEnum.TRANSFERENCE, // Tipo inválido
                Amount = 100.50,
                Description = "Transferência",
                StartDate = DateTime.UtcNow.AddDays(1),
                AccountId = 1,
                CategoryId = 1,
                RecurrenceRule = new CreateRecurrenceRuleRequest
                {
                    Frequency = RecurrenceFrequencyEnum.MONTHLY,
                    DayOfMonth = 15,
                    Interval = 1
                }
            };

            // Act
            var result = RecurringTransactionValidations.ValidateCreateRecurringTransactionRequest(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorMessage.Should().Be("Transações recorrentes só podem ser do tipo EXPENSE ou INCOME.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        [InlineData(-0.01)]
        public void ValidateCreateRecurringTransactionRequest_WithInvalidAmount_ShouldReturnError(double amount)
        {
            // Arrange
            var request = new CreateRecurringTransactionRequest
            {
                Type = TransactionTypeEnum.EXPENSE,
                Amount = amount,
                Description = "Despesa",
                StartDate = DateTime.UtcNow.AddDays(1),
                AccountId = 1,
                CategoryId = 1,
                RecurrenceRule = new CreateRecurrenceRuleRequest
                {
                    Frequency = RecurrenceFrequencyEnum.MONTHLY,
                    DayOfMonth = 15,
                    Interval = 1
                }
            };

            // Act
            var result = RecurringTransactionValidations.ValidateCreateRecurringTransactionRequest(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorMessage.Should().Be("Valor deve ser maior que zero.");
        }

        [Fact]
        public void ValidateCreateRecurringTransactionRequest_WithPastStartDate_ShouldReturnError()
        {
            // Arrange
            var request = new CreateRecurringTransactionRequest
            {
                Type = TransactionTypeEnum.EXPENSE,
                Amount = 100.50,
                Description = "Despesa",
                StartDate = DateTime.UtcNow.AddDays(-1), // Data no passado
                AccountId = 1,
                CategoryId = 1,
                RecurrenceRule = new CreateRecurrenceRuleRequest
                {
                    Frequency = RecurrenceFrequencyEnum.MONTHLY,
                    DayOfMonth = 15,
                    Interval = 1
                }
            };

            // Act
            var result = RecurringTransactionValidations.ValidateCreateRecurringTransactionRequest(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorMessage.Should().Be("Data de início não pode ser no passado.");
        }

        [Fact]
        public void ValidateCreateRecurringTransactionRequest_WithEndDateBeforeStartDate_ShouldReturnError()
        {
            // Arrange
            var request = new CreateRecurringTransactionRequest
            {
                Type = TransactionTypeEnum.EXPENSE,
                Amount = 100.50,
                Description = "Despesa",
                StartDate = DateTime.UtcNow.AddDays(10),
                EndDate = DateTime.UtcNow.AddDays(5), // Fim antes do início
                AccountId = 1,
                CategoryId = 1,
                RecurrenceRule = new CreateRecurrenceRuleRequest
                {
                    Frequency = RecurrenceFrequencyEnum.MONTHLY,
                    DayOfMonth = 15,
                    Interval = 1
                }
            };

            // Act
            var result = RecurringTransactionValidations.ValidateCreateRecurringTransactionRequest(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorMessage.Should().Be("Data de fim deve ser posterior à data de início.");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void ValidateCreateRecurringTransactionRequest_WithInvalidDescription_ShouldReturnError(string description)
        {
            // Arrange
            var request = new CreateRecurringTransactionRequest
            {
                Type = TransactionTypeEnum.EXPENSE,
                Amount = 100.50,
                Description = description,
                StartDate = DateTime.UtcNow.AddDays(1),
                AccountId = 1,
                CategoryId = 1,
                RecurrenceRule = new CreateRecurrenceRuleRequest
                {
                    Frequency = RecurrenceFrequencyEnum.MONTHLY,
                    DayOfMonth = 15,
                    Interval = 1
                }
            };

            // Act
            var result = RecurringTransactionValidations.ValidateCreateRecurringTransactionRequest(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorMessage.Should().Be("Descrição é obrigatória.");
        }

        [Fact]
        public void ValidateCreateRecurringTransactionRequest_WithInvalidRecurrenceRule_ShouldReturnError()
        {
            // Arrange
            var request = new CreateRecurringTransactionRequest
            {
                Type = TransactionTypeEnum.EXPENSE,
                Amount = 100.50,
                Description = "Despesa",
                StartDate = DateTime.UtcNow.AddDays(1),
                AccountId = 1,
                CategoryId = 1,
                RecurrenceRule = new CreateRecurrenceRuleRequest
                {
                    Frequency = RecurrenceFrequencyEnum.MONTHLY,
                    DayOfMonth = 31, // Dia inválido para recorrência mensal
                    Interval = 1
                }
            };

            // Act
            var result = RecurringTransactionValidations.ValidateCreateRecurringTransactionRequest(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorMessage.Should().Be("Dia do mês deve estar entre 1-28 ou -1 para último dia do mês.");
        }

        #endregion

        #region Edge Cases Tests

        [Fact]
        public void ValidateCreateRecurringTransactionRequest_WithTodayStartDate_ShouldReturnSuccess()
        {
            // Arrange
            var request = new CreateRecurringTransactionRequest
            {
                Type = TransactionTypeEnum.EXPENSE,
                Amount = 100.50,
                Description = "Despesa",
                StartDate = DateTime.UtcNow.Date, // Data de hoje
                AccountId = 1,
                CategoryId = 1,
                RecurrenceRule = new CreateRecurrenceRuleRequest
                {
                    Frequency = RecurrenceFrequencyEnum.MONTHLY,
                    DayOfMonth = 15,
                    Interval = 1
                }
            };

            // Act
            var result = RecurringTransactionValidations.ValidateCreateRecurringTransactionRequest(request);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void ValidateCreateRecurringTransactionRequest_WithSameStartAndEndDate_ShouldReturnError()
        {
            // Arrange
            var sameDate = DateTime.UtcNow.AddDays(10);
            var request = new CreateRecurringTransactionRequest
            {
                Type = TransactionTypeEnum.EXPENSE,
                Amount = 100.50,
                Description = "Despesa",
                StartDate = sameDate,
                EndDate = sameDate, // Mesma data de início e fim
                AccountId = 1,
                CategoryId = 1,
                RecurrenceRule = new CreateRecurrenceRuleRequest
                {
                    Frequency = RecurrenceFrequencyEnum.MONTHLY,
                    DayOfMonth = 15,
                    Interval = 1
                }
            };

            // Act
            var result = RecurringTransactionValidations.ValidateCreateRecurringTransactionRequest(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorMessage.Should().Be("Data de fim deve ser posterior à data de início.");
        }

        [Theory]
        [InlineData(2)] // A cada 2 dias
        [InlineData(3)] // A cada 3 dias
        [InlineData(7)] // A cada semana
        public void ValidateRecurrenceRuleRequest_WithValidInterval_ShouldReturnSuccess(int interval)
        {
            // Arrange
            var request = new CreateRecurrenceRuleRequest
            {
                Frequency = RecurrenceFrequencyEnum.DAILY,
                IsEveryDay = true,
                Interval = interval
            };

            // Act
            var result = RecurringTransactionValidations.ValidateRecurrenceRuleRequest(request);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        #endregion
    }
}
