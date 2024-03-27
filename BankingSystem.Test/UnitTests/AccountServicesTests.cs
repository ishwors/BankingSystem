using AutoMapper;
using BankingSystem.API.Data.Repository.IRepository;
using BankingSystem.API.DTOs;
using BankingSystem.API.Entities;
using BankingSystem.API.Services;
using BankingSystem.API.Services.IServices;
using Moq;
using Xunit;

namespace BankingSystem.Test.UnitTests
{
    public class AccountServicesTests
    {

        [Fact]
        public async Task AddAccountsValidInputReturnsAddedAccount()
        {
            // Arrange
            var mockAccountRepository = new Mock<IAccountRepository>();
            var userRepository= new Mock<IUserRepository>();
            var mockEmailService = new Mock<IEmailService>();

            var mockMapper = new Mock<IMapper>();

            var userCreationDTO = new UserCreationDTO
            (

                 username: "asdf",
              fullname: "hello world",
              email: "user@example.com",
             password: "hello",
             address: "earth",
             dateOfBirth: DateTime.UtcNow
                );

            var accounts = new Accounts
            {
                UserId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                AccountNumber = 1000010231785407,
                AtmCardNum = 9000099252964762,
                AtmCardPin = 1234,
                Balance = 123456,
                CreatedBy = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = null,
                ModifiedBy = null,
            };

            mockMapper.Setup(m => m.Map<Accounts>(accounts)).Returns(accounts);
            mockAccountRepository.Setup(r => r.AddAccounts(accounts)).ReturnsAsync(accounts);

            var accountService = new AccountServices(mockAccountRepository.Object, mockEmailService.Object, mockMapper.Object, userRepository.Object);

            // Act
            var result = await accountService.AddAccounts(accounts, userCreationDTO.Email);

            // Assert
            Assert.Equal(accounts, result);
            mockAccountRepository.Verify(r => r.AddAccounts(accounts), Times.Once);
            mockEmailService.Verify(e => e.SendEmailAsync(It.IsAny<Email>()), Times.Once);
        }

        // Add more tests for other methods as needed
    }
}
