using Xunit;
using Moq;
using FluentAssertions;
using ControleCerto.Controllers;
using ControleCerto.DTOs.Investment;
using ControleCerto.Errors;
using ControleCerto.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace ControleCerto.Tests
{
    public class InvestmentControllerTests
    {
        private readonly Mock<IInvestmentService> _serviceMock;
        private readonly InvestmentController _controller;

        public InvestmentControllerTests()
        {
            _serviceMock = new Mock<IInvestmentService>();
            _controller = new InvestmentController(_serviceMock.Object);

            // Mock HttpContext e Items
            var context = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            context.Items["UserId"] = 1;
            _controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = context
            };
        }

        [Fact]
        public async Task CreateInvestment_WithValidRequest_ShouldReturnCreatedResult()
        {
            // Arrange
            var request = new CreateInvestmentRequest
            {
                Name = "Fundo Imobiliário",
                InitialAmount = 100000,
                Description = "Investimento em FII"
            };

            var response = new InfoInvestmentResponse
            {
                Id = 1,
                Name = request.Name,
                CurrentValue = 100000,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _serviceMock.Setup(s => s.CreateInvestmentAsync(request, 1))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateInvestment(request);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
            createdResult.StatusCode.Should().Be(201);
            createdResult.Value.Should().Be(response);
            _serviceMock.Verify(s => s.CreateInvestmentAsync(request, 1), Times.Once);
        }

        [Fact]
        public async Task CreateInvestment_WithServiceError_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new CreateInvestmentRequest
            {
                Name = "Investimento",
                InitialAmount = 100000
            };

            var error = new AppError("Erro ao criar investimento", ControleCerto.Enums.ErrorTypeEnum.BusinessRule);
            _serviceMock.Setup(s => s.CreateInvestmentAsync(request, 1))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.CreateInvestment(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateInvestment_WithValidRequest_ShouldReturnOkResult()
        {
            // Arrange
            var request = new UpdateInvestmentRequest
            {
                Id = 1,
                Name = "Novo Nome",
                Description = "Nova Descrição"
            };

            var response = new InfoInvestmentResponse
            {
                Id = 1,
                Name = request.Name,
                CurrentValue = 100000,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _serviceMock.Setup(s => s.UpdateInvestmentAsync(request, 1))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UpdateInvestment(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().Be(response);
            _serviceMock.Verify(s => s.UpdateInvestmentAsync(request, 1), Times.Once);
        }

        [Fact]
        public async Task UpdateInvestment_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var request = new UpdateInvestmentRequest
            {
                Id = 999,
                Name = "Nome"
            };

            var error = new AppError("Investimento não encontrado", ControleCerto.Enums.ErrorTypeEnum.NotFound);
            _serviceMock.Setup(s => s.UpdateInvestmentAsync(request, 1))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.UpdateInvestment(request);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Deposit_WithValidRequest_ShouldReturnCreatedResult()
        {
            // Arrange
            var request = new DepositInvestmentRequest
            {
                InvestmentId = 1,
                Amount = 5000,
                Note = "Aporte mensal"
            };

            var response = new InfoInvestmentResponse
            {
                Id = 1,
                Name = "Investimento",
                CurrentValue = 105000,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _serviceMock.Setup(s => s.DepositAsync(request, 1))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Deposit(request);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
            createdResult.StatusCode.Should().Be(201);
            createdResult.Value.Should().Be(response);
            _serviceMock.Verify(s => s.DepositAsync(request, 1), Times.Once);
        }

        [Fact]
        public async Task Deposit_WithNegativeAmount_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new DepositInvestmentRequest
            {
                InvestmentId = 1,
                Amount = -1000
            };

            var error = new AppError("Valor inválido", ControleCerto.Enums.ErrorTypeEnum.Validation);
            _serviceMock.Setup(s => s.DepositAsync(request, 1))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.Deposit(request);

            // Assert
            result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [Fact]
        public async Task Withdraw_WithValidRequest_ShouldReturnCreatedResult()
        {
            // Arrange
            var request = new DepositInvestmentRequest
            {
                InvestmentId = 1,
                Amount = 20000,
                Note = "Resgate parcial"
            };

            var response = new InfoInvestmentResponse
            {
                Id = 1,
                Name = "Investimento",
                CurrentValue = 80000,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _serviceMock.Setup(s => s.WithdrawAsync(request, 1))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Withdraw(request);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
            createdResult.StatusCode.Should().Be(201);
            createdResult.Value.Should().Be(response);
            _serviceMock.Verify(s => s.WithdrawAsync(request, 1), Times.Once);
        }

        [Fact]
        public async Task AdjustValue_WithValidRequest_ShouldReturnCreatedResult()
        {
            // Arrange
            var request = new AdjustInvestmentRequest
            {
                InvestmentId = 1,
                NewTotalValue = 106000,
                Note = "Rendimento do mês"
            };

            var response = new InfoInvestmentResponse
            {
                Id = 1,
                Name = "Investimento",
                CurrentValue = 106000,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _serviceMock.Setup(s => s.AdjustInvestmentAsync(request, 1))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.AdjustValue(request);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
            createdResult.StatusCode.Should().Be(201);
            createdResult.Value.Should().Be(response);
            _serviceMock.Verify(s => s.AdjustInvestmentAsync(request, 1), Times.Once);
        }

        [Fact]
        public async Task AdjustValue_WithNegativeValue_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new AdjustInvestmentRequest
            {
                InvestmentId = 1,
                NewTotalValue = -1000
            };

            var error = new AppError("Valor inválido", ControleCerto.Enums.ErrorTypeEnum.Validation);
            _serviceMock.Setup(s => s.AdjustInvestmentAsync(request, 1))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.AdjustValue(request);

            // Assert
            result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [Fact]
        public async Task GetInvestments_ShouldReturnOkResult()
        {
            // Arrange
            var investments = new List<InfoInvestmentResponse>
            {
                new InfoInvestmentResponse { Id = 1, Name = "Inv 1", CurrentValue = 100000 },
                new InfoInvestmentResponse { Id = 2, Name = "Inv 2", CurrentValue = 50000 }
            };

            _serviceMock.Setup(s => s.GetInvestmentsAsync(1))
                .ReturnsAsync(investments);

            // Act
            var result = await _controller.GetInvestments();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            var returnedInvestments = (IEnumerable<InfoInvestmentResponse>)okResult.Value;
            returnedInvestments.Should().HaveCount(2);
            _serviceMock.Verify(s => s.GetInvestmentsAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetInvestmentHistory_WithValidId_ShouldReturnOkResult()
        {
            // Arrange
            var histories = new List<InvestmentHistoryResponse>
            {
                new InvestmentHistoryResponse { Id = 1, ChangeAmount = 100000, TotalValue = 100000, Type = "INVEST", OccurredAt = DateTime.UtcNow.AddDays(-2) },
                new InvestmentHistoryResponse { Id = 2, ChangeAmount = 5000, TotalValue = 105000, Type = "INVEST", OccurredAt = DateTime.UtcNow.AddDays(-1) },
                new InvestmentHistoryResponse { Id = 3, ChangeAmount = 1000, TotalValue = 106000, Type = "ADJUSTMENT", OccurredAt = DateTime.UtcNow }
            };

            _serviceMock.Setup(s => s.GetInvestmentHistoryAsync(1, 1))
                .ReturnsAsync(histories);

            // Act
            var result = await _controller.GetInvestmentHistory(1);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            var returnedHistories = (IEnumerable<InvestmentHistoryResponse>)okResult.Value;
            returnedHistories.Should().HaveCount(3);
            _serviceMock.Verify(s => s.GetInvestmentHistoryAsync(1, 1), Times.Once);
        }

        [Fact]
        public async Task GetInvestmentHistory_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var error = new AppError("Investimento não encontrado", ControleCerto.Enums.ErrorTypeEnum.NotFound);
            _serviceMock.Setup(s => s.GetInvestmentHistoryAsync(999, 1))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.GetInvestmentHistory(999);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task CreateInvestment_ExtractsUserIdFromHttpContext()
        {
            // Arrange
            var request = new CreateInvestmentRequest
            {
                Name = "Teste",
                InitialAmount = 1000
            };

            var response = new InfoInvestmentResponse
            {
                Id = 1,
                Name = "Teste",
                CurrentValue = 1000
            };

            _serviceMock.Setup(s => s.CreateInvestmentAsync(request, 1))
                .ReturnsAsync(response);

            // Act
            await _controller.CreateInvestment(request);

            // Assert
            _serviceMock.Verify(s => s.CreateInvestmentAsync(request, 1), Times.Once);
        }

        [Fact]
        public async Task GetInvestments_ShouldReturnEmptyListWhenNoInvestments()
        {
            // Arrange
            var emptyList = new List<InfoInvestmentResponse>();

            _serviceMock.Setup(s => s.GetInvestmentsAsync(1))
                .ReturnsAsync(emptyList);

            // Act
            var result = await _controller.GetInvestments();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedInvestments = (IEnumerable<InfoInvestmentResponse>)okResult.Value;
            returnedInvestments.Should().BeEmpty();
        }

        [Fact]
        public async Task MultipleOperations_ShouldUseCorrectUserId()
        {
            // Arrange
            var depositRequest = new DepositInvestmentRequest
            {
                InvestmentId = 1,
                Amount = 5000
            };

            var adjustRequest = new AdjustInvestmentRequest
            {
                InvestmentId = 1,
                NewTotalValue = 106000
            };

            var response = new InfoInvestmentResponse
            {
                Id = 1,
                CurrentValue = 105000
            };

            _serviceMock.Setup(s => s.DepositAsync(depositRequest, 1))
                .ReturnsAsync(response);

            _serviceMock.Setup(s => s.AdjustInvestmentAsync(adjustRequest, 1))
                .ReturnsAsync(new InfoInvestmentResponse { Id = 1, CurrentValue = 106000 });

            // Act
            await _controller.Deposit(depositRequest);
            await _controller.AdjustValue(adjustRequest);

            // Assert
            _serviceMock.Verify(s => s.DepositAsync(It.IsAny<DepositInvestmentRequest>(), 1), Times.Once);
            _serviceMock.Verify(s => s.AdjustInvestmentAsync(It.IsAny<AdjustInvestmentRequest>(), 1), Times.Once);
        }
    }
}
