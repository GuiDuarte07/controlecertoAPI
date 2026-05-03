using Xunit;
using Moq;
using FluentAssertions;
using ControleCerto.Models.AppDbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using ControleCerto.Modules.Tickets.Services;
using ControleCerto.Modules.Tickets.DTOs;
using ControleCerto.Services.Interfaces;
using ControleCerto.Models.Entities;
using ControleCerto.Enums;
using Microsoft.AspNetCore.Http;

namespace ControleCerto.Tests
{
    public class TicketsServiceTests
    {
        private readonly AppDbContext _dbContext;
        private readonly Mock<IS3Service> _s3Mock;
        private readonly Mock<IEmailService> _emailMock;
        private readonly IConfiguration _configuration;
        private readonly TicketsService _service;

        public TicketsServiceTests()
        {
            _s3Mock = new Mock<IS3Service>();
            _emailMock = new Mock<IEmailService>();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TicketsTestDb_{Guid.NewGuid():N}")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _configuration = BuildConfiguration();
            _dbContext = new AppDbContext(options, _configuration);
            _dbContext.Database.EnsureDeleted();
            _dbContext.Database.EnsureCreated();

            _service = new TicketsService(_dbContext, _s3Mock.Object, _emailMock.Object, _configuration);
        }

        private static IConfiguration BuildConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                { "ConnectionStrings:WebApiDatabase", "Test" },
                { "ConnectionStrings:WebSiteUrl", "https://app.test" }
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();
        }

        private static IFormFile CreateTestFile(string fileName, string contentType, byte[] content)
        {
            var stream = new MemoryStream(content);
            return new FormFile(stream, 0, content.Length, "Attachments", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        [Fact]
        public async Task CreateTicketAsync_WithInitialMessage_ShouldCreateTicketAndNotifyAdmins()
        {
            var user = new User { Id = 10, Name = "User", Email = "user@test.com", PasswordHash = "x" };
            var admin = new User { Id = 20, Name = "Admin", Email = "admin@test.com", PasswordHash = "x", IsAdmin = true };
            await _dbContext.Users.AddRangeAsync(user, admin);
            await _dbContext.SaveChangesAsync();

            var request = new CreateTicketRequest
            {
                Subject = "Problema ao acessar",
                Description = "<p>Não consigo acessar</p>",
                Attachments = null
            };

            var result = await _service.CreateTicketAsync(request, user.Id);

            result.IsSuccess.Should().BeTrue();
            result.Value.Subject.Should().Be("Problema ao acessar");
            result.Value.Messages.Should().HaveCount(1);

            var ticket = await _dbContext.Tickets.FirstAsync();
            ticket.UserId.Should().Be(user.Id);

            var notifications = await _dbContext.Notifications.Where(n => n.UserId == admin.Id).ToListAsync();
            notifications.Should().HaveCount(1);
            notifications[0].Type.Should().Be(NotificationTypeEnum.TICKETUPDATE);
            notifications[0].ActionPath.Should().Be($"/admin/tickets/{ticket.Id}");

            _emailMock.Verify(m => m.SendEmail(
                It.Is<List<string>>(list => list.Contains(admin.Email)),
                It.IsAny<string>(),
                It.IsAny<string>()
            ), Times.Once);
        }

        [Fact]
        public async Task CreateTicketAsync_WithAttachment_ShouldUploadToS3AndPersistMetadata()
        {
            var user = new User { Id = 11, Name = "User", Email = "user2@test.com", PasswordHash = "x" };
            var admin = new User { Id = 21, Name = "Admin", Email = "admin2@test.com", PasswordHash = "x", UserType = UserTypeEnum.ADMIN };
            await _dbContext.Users.AddRangeAsync(user, admin);
            await _dbContext.SaveChangesAsync();

            _s3Mock.Setup(s => s.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync("https://s3.test/tickets/file");

            var file = CreateTestFile("print.png", "image/png", new byte[] { 1, 2, 3, 4 });
            var request = new CreateTicketRequest
            {
                Subject = "Bug",
                Description = "<p>Detalhes</p>",
                Attachments = new[] { file }
            };

            var result = await _service.CreateTicketAsync(request, user.Id);

            result.IsSuccess.Should().BeTrue();

            var attachments = await _dbContext.TicketAttachments.ToListAsync();
            attachments.Should().HaveCount(1);
            attachments[0].FileName.Should().Be("print.png");
            attachments[0].Url.Should().Be("https://s3.test/tickets/file");

            _s3Mock.Verify(s => s.UploadFileAsync(It.IsAny<IFormFile>(), It.Is<string>(k => k.StartsWith("tickets/"))), Times.Once);
        }
    }
}

