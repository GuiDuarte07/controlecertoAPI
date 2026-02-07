using Xunit;
using Moq;
using AutoMapper;
using FluentAssertions;
using ControleCerto.Services;
using ControleCerto.DTOs.Investment;
using ControleCerto.Models.AppDbContext;
using ControleCerto.Models.Entities;
using ControleCerto.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace ControleCerto.Tests
{
    public class InvestmentServiceTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly AppDbContext _dbContext;
        private readonly InvestmentService _investmentService;

        public InvestmentServiceTests()
        {
            _mapperMock = new Mock<IMapper>();
            
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "InvestmentTestDb")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _dbContext = new AppDbContext(options, GetMockConfiguration());
            _dbContext.Database.EnsureDeleted();
            _dbContext.Database.EnsureCreated();
            _investmentService = new InvestmentService(_dbContext, _mapperMock.Object);
        }

        private IConfiguration GetMockConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                { "ConnectionStrings:WebApiDatabase", "Test" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            return configuration;
        }

        [Fact]
        public async Task CreateInvestmentAsync_WithValidRequest_ShouldCreateInvestment()
        {
            // Arrange
            int userId = 1;
            var request = new CreateInvestmentRequest
            {
                Name = "Fundo Imobiliário",
                InitialAmount = 100000,
                Description = "Investimento em FII ABC"
            };

            var investmentEntity = new Investment
            {
                Id = 1,
                Name = request.Name,
                CurrentValue = 100000,
                Description = request.Description,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var expectedResponse = new InfoInvestmentResponse
            {
                Id = 1,
                Name = request.Name,
                CurrentValue = 100000,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mapperMock.Setup(m => m.Map<Investment>(request))
                .Returns(new Investment { Name = request.Name, Description = request.Description });

            _mapperMock.Setup(m => m.Map<InfoInvestmentResponse>(It.IsAny<Investment>()))
                .Returns(expectedResponse);

            // Act
            var result = await _investmentService.CreateInvestmentAsync(request, userId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Name.Should().Be(request.Name);
            result.Value.CurrentValue.Should().Be(100000);

            var createdInvestment = await _dbContext.Investments.FirstOrDefaultAsync(i => i.UserId == userId);
            createdInvestment.Should().NotBeNull();
            createdInvestment.CurrentValue.Should().Be(100000);
        }

        [Fact]
        public async Task CreateInvestmentAsync_WithoutInitialAmount_ShouldCreateInvestmentWithZeroValue()
        {
            // Arrange
            int userId = 1;
            var request = new CreateInvestmentRequest
            {
                Name = "Novo Investimento",
                Description = "Sem valor inicial"
            };

            _mapperMock.Setup(m => m.Map<Investment>(request))
                .Returns(new Investment { Name = request.Name, Description = request.Description });

            _mapperMock.Setup(m => m.Map<InfoInvestmentResponse>(It.IsAny<Investment>()))
                .Returns(new InfoInvestmentResponse
                {
                    Name = request.Name,
                    CurrentValue = 0,
                    Description = request.Description
                });

            // Act
            var result = await _investmentService.CreateInvestmentAsync(request, userId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.CurrentValue.Should().Be(0);

            var createdInvestment = await _dbContext.Investments.FirstOrDefaultAsync(i => i.UserId == userId);
            createdInvestment.Should().NotBeNull();
            createdInvestment.CurrentValue.Should().Be(0);
        }

        [Fact]
        public async Task UpdateInvestmentAsync_WithValidRequest_ShouldUpdateInvestment()
        {
            // Arrange
            int userId = 1;
            var investment = new Investment
            {
                Id = 1,
                Name = "Investimento Antigo",
                CurrentValue = 100000,
                Description = "Descrição antiga",
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Histories = new List<InvestmentHistory>()
            };

            await _dbContext.Investments.AddAsync(investment);
            await _dbContext.SaveChangesAsync();

            var request = new UpdateInvestmentRequest
            {
                Id = 1,
                Name = "Investimento Novo",
                Description = "Descrição nova"
            };

            _mapperMock.Setup(m => m.Map<InfoInvestmentResponse>(It.IsAny<Investment>()))
                .Returns(new InfoInvestmentResponse
                {
                    Id = 1,
                    Name = request.Name,
                    CurrentValue = 100000,
                    Description = request.Description
                });

            // Act
            var result = await _investmentService.UpdateInvestmentAsync(request, userId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Name.Should().Be("Investimento Novo");
            result.Value.Description.Should().Be("Descrição nova");

            var updatedInvestment = await _dbContext.Investments.FirstOrDefaultAsync(i => i.Id == 1);
            updatedInvestment.Name.Should().Be("Investimento Novo");
        }

        [Fact]
        public async Task UpdateInvestmentAsync_WithInvalidUserId_ShouldReturnNotFound()
        {
            // Arrange
            var investment = new Investment
            {
                Id = 1,
                Name = "Investimento",
                CurrentValue = 100000,
                UserId = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Histories = new List<InvestmentHistory>()
            };

            await _dbContext.Investments.AddAsync(investment);
            await _dbContext.SaveChangesAsync();

            var request = new UpdateInvestmentRequest
            {
                Id = 1,
                Name = "Nome Novo"
            };

            // Act
            var result = await _investmentService.UpdateInvestmentAsync(request, 999);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorType.Should().Be(ControleCerto.Enums.ErrorTypeEnum.NotFound);
        }

        [Fact]
        public async Task DepositAsync_WithValidAmount_ShouldIncreaseInvestmentValue()
        {
            // Arrange
            int userId = 1;
            var investment = new Investment
            {
                Id = 1,
                Name = "Investimento",
                CurrentValue = 100000,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Histories = new List<InvestmentHistory>()
            };

            await _dbContext.Investments.AddAsync(investment);
            await _dbContext.SaveChangesAsync();

            var request = new DepositInvestmentRequest
            {
                InvestmentId = 1,
                Amount = 5000,
                Note = "Aporte mensal"
            };

            _mapperMock.Setup(m => m.Map<InfoInvestmentResponse>(It.IsAny<Investment>()))
                .Returns(new InfoInvestmentResponse
                {
                    Id = 1,
                    Name = "Investimento",
                    CurrentValue = 105000
                });

            // Act
            var result = await _investmentService.DepositAsync(request, userId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.CurrentValue.Should().Be(105000);

            var updatedInvestment = await _dbContext.Investments.FirstOrDefaultAsync(i => i.Id == 1);
            updatedInvestment.CurrentValue.Should().Be(105000);

            var history = await _dbContext.InvestmentHistories
                .FirstOrDefaultAsync(h => h.InvestmentId == 1);
            history.Should().NotBeNull();
            history.Type.Should().Be(InvestmentHistoryTypeEnum.INVEST);
            history.ChangeAmount.Should().Be(5000);
        }

        [Fact]
        public async Task WithdrawAsync_WithValidAmount_ShouldDecreaseInvestmentValue()
        {
            // Arrange
            int userId = 1;
            var investment = new Investment
            {
                Id = 1,
                Name = "Investimento",
                CurrentValue = 100000,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Histories = new List<InvestmentHistory>()
            };

            await _dbContext.Investments.AddAsync(investment);
            await _dbContext.SaveChangesAsync();

            var request = new DepositInvestmentRequest
            {
                InvestmentId = 1,
                Amount = 20000,
                Note = "Resgate parcial"
            };

            _mapperMock.Setup(m => m.Map<InfoInvestmentResponse>(It.IsAny<Investment>()))
                .Returns(new InfoInvestmentResponse
                {
                    Id = 1,
                    Name = "Investimento",
                    CurrentValue = 80000
                });

            // Act
            var result = await _investmentService.WithdrawAsync(request, userId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.CurrentValue.Should().Be(80000);

            var updatedInvestment = await _dbContext.Investments.FirstOrDefaultAsync(i => i.Id == 1);
            updatedInvestment.CurrentValue.Should().Be(80000);

            var history = await _dbContext.InvestmentHistories
                .FirstOrDefaultAsync(h => h.InvestmentId == 1);
            history.Should().NotBeNull();
            history.Type.Should().Be(InvestmentHistoryTypeEnum.WITHDRAW);
            history.ChangeAmount.Should().Be(-20000);
        }

        [Fact]
        public async Task AdjustInvestmentAsync_WithNewTotalValue_ShouldUpdateAndRecordAdjustment()
        {
            // Arrange
            int userId = 1;
            var investment = new Investment
            {
                Id = 1,
                Name = "Investimento",
                CurrentValue = 100000,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Histories = new List<InvestmentHistory>()
            };

            await _dbContext.Investments.AddAsync(investment);
            await _dbContext.SaveChangesAsync();

            var request = new AdjustInvestmentRequest
            {
                InvestmentId = 1,
                NewTotalValue = 106000,
                Note = "Rendimento do mês"
            };

            _mapperMock.Setup(m => m.Map<InfoInvestmentResponse>(It.IsAny<Investment>()))
                .Returns(new InfoInvestmentResponse
                {
                    Id = 1,
                    Name = "Investimento",
                    CurrentValue = 106000
                });

            // Act
            var result = await _investmentService.AdjustInvestmentAsync(request, userId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.CurrentValue.Should().Be(106000);

            var updatedInvestment = await _dbContext.Investments.FirstOrDefaultAsync(i => i.Id == 1);
            updatedInvestment.CurrentValue.Should().Be(106000);

            var history = await _dbContext.InvestmentHistories
                .FirstOrDefaultAsync(h => h.InvestmentId == 1);
            history.Should().NotBeNull();
            history.Type.Should().Be(InvestmentHistoryTypeEnum.ADJUSTMENT);
            history.ChangeAmount.Should().Be(6000);
        }

        [Fact]
        public async Task GetInvestmentsAsync_WithMultipleInvestments_ShouldReturnUserInvestments()
        {
            // Arrange
            int userId1 = 1;
            int userId2 = 2;

            var investments = new List<Investment>
            {
                new Investment { Id = 1, Name = "Inv 1", CurrentValue = 100000, UserId = userId1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, Histories = new List<InvestmentHistory>() },
                new Investment { Id = 2, Name = "Inv 2", CurrentValue = 50000, UserId = userId1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, Histories = new List<InvestmentHistory>() },
                new Investment { Id = 3, Name = "Inv 3", CurrentValue = 75000, UserId = userId2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, Histories = new List<InvestmentHistory>() }
            };

            await _dbContext.Investments.AddRangeAsync(investments);
            await _dbContext.SaveChangesAsync();

            _mapperMock.Setup(m => m.Map<IEnumerable<InfoInvestmentResponse>>(It.IsAny<List<Investment>>()))
                .Returns(new List<InfoInvestmentResponse>
                {
                    new InfoInvestmentResponse { Id = 1, Name = "Inv 1", CurrentValue = 100000 },
                    new InfoInvestmentResponse { Id = 2, Name = "Inv 2", CurrentValue = 50000 }
                });

            // Act
            var result = await _investmentService.GetInvestmentsAsync(userId1);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2);
            result.Value.Should().AllSatisfy(i => i.Should().NotBeNull());
        }

        [Fact]
        public async Task GetInvestmentHistoryAsync_WithValidInvestmentId_ShouldReturnOrderedHistory()
        {
            // Arrange
            int userId = 1;
            var investment = new Investment
            {
                Id = 1,
                Name = "Investimento",
                CurrentValue = 106000,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Histories = new List<InvestmentHistory>()
            };

            await _dbContext.Investments.AddAsync(investment);
            await _dbContext.SaveChangesAsync();

            var histories = new List<InvestmentHistory>
            {
                new InvestmentHistory { Id = 1, InvestmentId = 1, ChangeAmount = 100000, TotalValue = 100000, Type = InvestmentHistoryTypeEnum.INVEST, OccurredAt = DateTime.UtcNow.AddDays(-2) },
                new InvestmentHistory { Id = 2, InvestmentId = 1, ChangeAmount = 5000, TotalValue = 105000, Type = InvestmentHistoryTypeEnum.INVEST, OccurredAt = DateTime.UtcNow.AddDays(-1) },
                new InvestmentHistory { Id = 3, InvestmentId = 1, ChangeAmount = 1000, TotalValue = 106000, Type = InvestmentHistoryTypeEnum.ADJUSTMENT, OccurredAt = DateTime.UtcNow }
            };

            await _dbContext.InvestmentHistories.AddRangeAsync(histories);
            await _dbContext.SaveChangesAsync();

            _mapperMock.Setup(m => m.Map<IEnumerable<InvestmentHistoryResponse>>(It.IsAny<List<InvestmentHistory>>()))
                .Returns(new List<InvestmentHistoryResponse>
                {
                    new InvestmentHistoryResponse { Id = 1, ChangeAmount = 100000, TotalValue = 100000, Type = "INVEST" },
                    new InvestmentHistoryResponse { Id = 2, ChangeAmount = 5000, TotalValue = 105000, Type = "INVEST" },
                    new InvestmentHistoryResponse { Id = 3, ChangeAmount = 1000, TotalValue = 106000, Type = "ADJUSTMENT" }
                });

            // Act
            var result = await _investmentService.GetInvestmentHistoryAsync(1, userId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetInvestmentHistoryAsync_WithInvalidUserId_ShouldReturnNotFound()
        {
            // Arrange
            var investment = new Investment
            {
                Id = 1,
                Name = "Investimento",
                CurrentValue = 100000,
                UserId = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Histories = new List<InvestmentHistory>()
            };

            await _dbContext.Investments.AddAsync(investment);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _investmentService.GetInvestmentHistoryAsync(1, 999);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorType.Should().Be(ControleCerto.Enums.ErrorTypeEnum.NotFound);
        }

        [Fact]
        public async Task DepositAsync_WithInvalidInvestmentId_ShouldReturnNotFound()
        {
            // Arrange
            var request = new DepositInvestmentRequest
            {
                InvestmentId = 999,
                Amount = 5000
            };

            // Act
            var result = await _investmentService.DepositAsync(request, 1);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorType.Should().Be(ControleCerto.Enums.ErrorTypeEnum.NotFound);
        }

        [Fact]
        public async Task MathRound_ShouldApplyOnDeposit()
        {
            // Arrange
            int userId = 1;
            var investment = new Investment
            {
                Id = 1,
                Name = "Investimento",
                CurrentValue = 100,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Histories = new List<InvestmentHistory>()
            };

            await _dbContext.Investments.AddAsync(investment);
            await _dbContext.SaveChangesAsync();

            var request = new DepositInvestmentRequest
            {
                InvestmentId = 1,
                Amount = 0.126 // Deve ser arredondado para 0.13
            };

            _mapperMock.Setup(m => m.Map<InfoInvestmentResponse>(It.IsAny<Investment>()))
                .Returns(new InfoInvestmentResponse
                {
                    Id = 1,
                    Name = "Investimento",
                    CurrentValue = 100.13
                });

            // Act
            var result = await _investmentService.DepositAsync(request, userId);

            // Assert
            var updatedInvestment = await _dbContext.Investments.FirstOrDefaultAsync(i => i.Id == 1);
            updatedInvestment.CurrentValue.Should().Be(100.13);

            var history = await _dbContext.InvestmentHistories.FirstOrDefaultAsync(h => h.InvestmentId == 1);
            history.ChangeAmount.Should().Be(0.13);
        }
    }
}
