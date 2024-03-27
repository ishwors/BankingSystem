using AutoMapper;
using BankingSystem.API.Data.Repository.IRepository;
using BankingSystem.API.DTOs;
using BankingSystem.API.Entities;
using BankingSystem.API.Services;
using BankingSystem.API.Services.IServices;
using BankingSystem.API.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Globalization;

namespace BankingSystem.Test.UnitTests
{
    public class UserServiceTests
    {
        [Fact]
        public async Task RegisterUser_WithValidData_ShouldReturnUserInfoDisplayDTO()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UserCreationDTO, Users>();
                cfg.CreateMap<Users, UserInfoDisplayDTO>();
            });
            var mapper = mapperConfig.CreateMapper();

            var mapperConfig1 = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AccountDTO, Accounts>();
            });
            var mapper1 = mapperConfig1.CreateMapper();

            var accountRepositoryMock = new Mock<IAccountRepository>();
            var configurationMock = new Mock<IConfiguration>();
            var userManagerMock = MockUserManager<Users>();
            var signInManagerMock = MockSignInManager<Users>();
            var passwordHasherMock = new Mock<IPasswordHasher<Users>>();

            var emailServiceMock = new Mock<IEmailService>();
            // EmailService mock setup
            //emailServiceMock.Setup(es => es.SendEmailAsync(It.IsAny<Email>())).Returns(Task); // Mock the SendEmailAsync method
            emailServiceMock
            .Setup(es => es.SendEmailAsync(It.IsAny<Email>()))
            .ReturnsAsync("Email sent successfully."); // Return a completed Task<string> with the desired message


            var accountServicesMock = new Mock<IAccountService>();

            var contextMock = new Mock<IHttpContextAccessor>();
            var getLoggedInUserMock = new Mock<GetLoggedinUser>(contextMock.Object);

            var userService = new UserService(userRepositoryMock.Object, mapper, accountServicesMock.Object, userManagerMock.Object, signInManagerMock.Object, passwordHasherMock.Object, getLoggedInUserMock.Object);

            var userCreationDTO = new UserCreationDTO
            {
                UserName = "ishwor",
                Fullname = "Ishwor Shrestha",
                Address = "Pulchowk",
                Email = "ishwor@gmail.com",
                Password = "password", // Assuming you also set the password in the DTO
                PhoneNumber = "string",
                UserType = 0,
                DateOfBirth = DateTime.SpecifyKind(DateTime.ParseExact("2000-03-23T11:13:25.342Z", "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture), DateTimeKind.Utc)
            };

            userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync((Users)null);
            userRepositoryMock.Setup(repo => repo.AddUsers(It.IsAny<Users>())).ReturnsAsync(new Users());

            userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((Users)null);
            userManagerMock.Setup(um => um.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((Users)null);
            userManagerMock.Setup(um => um.CreateAsync(It.IsAny<Users>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            userManagerMock.Setup(um => um.GetRolesAsync(It.IsAny<Users>())).ReturnsAsync(new List<string>());

            signInManagerMock.Setup(sm => sm.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), true, false))
                .ReturnsAsync(SignInResult.Success);

            passwordHasherMock.Setup(ph => ph.HashPassword(It.IsAny<Users>(), It.IsAny<string>()))
                .Returns((Users user, string password) => password); // Mock password hashing

            // Act
            var result = await userService.RegisterUser(userCreationDTO);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Ishwor Shrestha", result.Fullname);
            Assert.Equal("ishwor", result.UserName);
        }

        // Helper method to mock UserManager
        public static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
        {
            var userStoreMock = new Mock<IUserStore<TUser>>();
            return new Mock<UserManager<TUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
        }

        // Helper method to mock SignInManager
        public static Mock<SignInManager<TUser>> MockSignInManager<TUser>() where TUser : class
        {
            var userStoreMock = new Mock<IUserStore<TUser>>();
            var userManagerMock = MockUserManager<TUser>();
            return new Mock<SignInManager<TUser>>(userManagerMock.Object, new HttpContextAccessor(), new Mock<IUserClaimsPrincipalFactory<TUser>>().Object, null, null, null, null);
        }


        [Fact]
        public async Task GetUserAsync_ReturnsUser()
        {
            // Arrange
            var Id = new Guid();
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();

            var mapperMock = new Mock<IMapper>();

            var mapperConfig1 = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AccountDTO, Accounts>();
            });
            var mapper1 = mapperConfig1.CreateMapper();

            var accountRepositoryMock = new Mock<IAccountRepository>();
            var configurationMock = new Mock<IConfiguration>();
            var userManagerMock = MockUserManager<Users>();
            var signInManagerMock = MockSignInManager<Users>();
            var passwordHasherMock = new Mock<IPasswordHasher<Users>>();

            var emailServiceMock = new Mock<IEmailService>();
            //emailServiceMock.Setup(es => es.SendEmailAsync(It.IsAny<Email>())).Returns(Task.CompletedTask); // Mock the SendEmailAsync method
            emailServiceMock
            .Setup(es => es.SendEmailAsync(It.IsAny<Email>()))
            .ReturnsAsync("Email sent successfully."); // Return a completed Task<string> with the desired message

            var accountServicesMock = new Mock<IAccountService>();
            var contextMock = new Mock<IHttpContextAccessor>();
            var getLoggedInUserMock = new Mock<GetLoggedinUser>(contextMock.Object);

            var userService = new UserService(userRepositoryMock.Object, mapperMock.Object, accountServicesMock.Object, userManagerMock.Object, signInManagerMock.Object, passwordHasherMock.Object, getLoggedInUserMock.Object);

            // Set up mocks
            userRepositoryMock.Setup(repo => repo.GetUserAsync(Id))
                .ReturnsAsync(new Users { Id = Id, UserName = "ishwor", Fullname = "Ishwor Shrestha", Address = "Pulchowk", Email = "ishwor@gmail.com" });

            // Act
            var result = await userService.GetUserAsync(Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Id, result.Id);
            Assert.Equal("Ishwor Shrestha", result.Fullname);
            Assert.Equal("ishwor", result.UserName);
        }

        [Fact]
        public async Task GetUsersAsync_AllReturnsUsers()
        {
            // Arrange
            var id1 = new Guid();
            var id2 = new Guid();

            var userRepositoryMock = new Mock<IUserRepository>();
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Users, UserInfoDisplayDTO>();
            });
            var mapper = mapperConfig.CreateMapper();

            var mapperConfig1 = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AccountDTO, Accounts>();
            });
            var mapper1 = mapperConfig1.CreateMapper();

            var accountRepositoryMock = new Mock<IAccountRepository>();
            var configurationMock = new Mock<IConfiguration>();
            var userManagerMock = MockUserManager<Users>();

            var signInManagerMock = MockSignInManager<Users>();
            var passwordHasherMock = new Mock<IPasswordHasher<Users>>();

            var emailServiceMock = new Mock<IEmailService>();
            //emailServiceMock.Setup(es => es.SendEmailAsync(It.IsAny<Email>())).Returns(Task.CompletedTask); // Mock the SendEmailAsync method
            emailServiceMock
            .Setup(es => es.SendEmailAsync(It.IsAny<Email>()))
            .ReturnsAsync("Email sent successfully."); // Return a completed Task<string> with the desired message

            var accountServicesMock = new Mock<IAccountService>();
            var contextMock = new Mock<IHttpContextAccessor>();
            var getLoggedInUserMock = new Mock<GetLoggedinUser>(contextMock.Object);

            var userService = new UserService(userRepositoryMock.Object, mapper, accountServicesMock.Object, userManagerMock.Object, signInManagerMock.Object, passwordHasherMock.Object, getLoggedInUserMock.Object);

            // Set up mocks
            var expectedUsers = new List<Users>
            {
                new Users {
                    Id= id1,
                    UserName = "ishwor",
                    Fullname = "Ishwor Shrestha",
                    Address = "Pulchowk",
                    Email = "ishwor@gmail.com",
                    PasswordHash = "password", // Assuming you also set the password in the DTO
                    PhoneNumber = "string",
                    DateOfBirth = DateTime.SpecifyKind(DateTime.ParseExact("2000-03-23T11:13:25.342Z", "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture), DateTimeKind.Utc)},
                new Users {
                    Id = id2, UserName = "ishwor2", Fullname = "Ishwor Shrestha 2", Address = "Pulchowk 2", Email = "ishwor2@gmail.com" }
            };

            // Configure userManagerMock to return expectedUsers when accessed
            userManagerMock.Setup(um => um.Users).Returns(expectedUsers.AsQueryable());
            // Mock GetRolesAsync method
            userManagerMock.Setup(um => um.GetRolesAsync(It.IsAny<Users>())).ReturnsAsync(new List<string> { "AccountHolder", "TellerPerson" });

            // Act
            var users = await userService.GetUsersAsync(); // Retrieve the task

            // Assert
            Assert.NotNull(users);
            var userList = users.ToList(); // Convert the IEnumerable to a List
            Assert.Equal(expectedUsers.Count, userList.Count);
            Assert.Equal(expectedUsers.First().Id, userList.First().Id);
            Assert.Equal(expectedUsers.First().Fullname, userList.First().Fullname);
            Assert.Equal(expectedUsers.Last().Id, userList.Last().Id);
            Assert.Equal(expectedUsers.Last().Fullname, userList.Last().Fullname);
        }

        [Fact]
        public async Task PatchUserDetails_WithValidData_ShouldReturnUpdatedUserInfoDisplayDTO()
        {
            // Arrange
            var Id = new Guid();
            var patchDocument = new JsonPatchDocument<UserCreationDTO>();
            // Assume patchDocument is properly configured with valid operations

            var userRepositoryMock = new Mock<IUserRepository>();
            var mapperMock = new Mock<IMapper>();
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Users, UserInfoDisplayDTO>();
            });
            var mapper = mapperConfig.CreateMapper();
            var accountRepositoryMock = new Mock<IAccountRepository>();
            var configurationMock = new Mock<IConfiguration>();
            var userManagerMock = MockUserManager<Users>();
            var signInManagerMock = MockSignInManager<Users>();
            var passwordHasherMock = new Mock<IPasswordHasher<Users>>();
            var emailServiceMock = new Mock<IEmailService>();
            var accountServicesMock = new Mock<IAccountService>();

            var contextMock = new Mock<IHttpContextAccessor>();
            var getLoggedInUserMock = new Mock<GetLoggedinUser>(contextMock.Object);

            var userService = new UserService(userRepositoryMock.Object, mapper, accountServicesMock.Object, userManagerMock.Object, signInManagerMock.Object, passwordHasherMock.Object, getLoggedInUserMock.Object);

            // Mock GetRolesAsync method
            userManagerMock.Setup(um => um.GetRolesAsync(It.IsAny<Users>())).ReturnsAsync(new List<string> { "AccountHolder", "TellerPerson" });

            // Setup UserRepository to return a user with given Id
            userRepositoryMock.Setup(repo => repo.PatchUserDetails(Id, patchDocument))
                .ReturnsAsync(new Users { Id = Id, UserName = "updatedUserName", Fullname = "Updated FullName", Address = "Updated Address", Email = "updatedemail@gmail.com" });

            // Act
            var result = await userService.PatchUserDetails(Id, patchDocument);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Id, result.Id);
            Assert.Equal("Updated FullName", result.Fullname);
            Assert.Equal("updatedUserName", result.UserName);
            // Assert other properties as needed
        }

        [Fact]
        public async Task UpdateUsersAsync_WithValidData_ShouldReturnUpdatedUserInfoDisplayDTO()
        {
            // Arrange
            var Id = new Guid();
            var userUpdateDTO = new UserUpdateDTO
            {
                UserName = "updatedUserName",
                Fullname = "Updated FullName",
                Address = "Updated Address",
                Email = "updatedemail@gmail.com"
                // Assuming other properties are properly configured
            };

            // Mock existing user
            var existingUser = new Users
            {
                Id = Id,
                UserName = "existingUserName",
                Fullname = "Existing FullName",
                Address = "Existing Address",
                Email = "existingemail@gmail.com",
                // Set other properties as needed
            };

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(repo => repo.GetUserAsync(Id)).ReturnsAsync(existingUser);

            var mapperMock = new Mock<IMapper>();
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UserUpdateDTO, Users>();
                cfg.CreateMap<Users, UserInfoDisplayDTO>();
            });
            var mapper = mapperConfig.CreateMapper();

            var mapperConfig1 = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Accounts, Accounts>();
            });
            var mapper1 = mapperConfig1.CreateMapper();

            var accountRepositoryMock = new Mock<IAccountRepository>();
            var configurationMock = new Mock<IConfiguration>();
            var userManagerMock = MockUserManager<Users>();
            var signInManagerMock = MockSignInManager<Users>();
            var passwordHasherMock = new Mock<IPasswordHasher<Users>>();
            var emailServiceMock = new Mock<IEmailService>();
            var accountServicesMock = new Mock<IAccountService>();
            var contextMock = new Mock<IHttpContextAccessor>();
            var getLoggedInUserMock = new Mock<GetLoggedinUser>(contextMock.Object);

            var userService = new UserService(userRepositoryMock.Object, mapper, accountServicesMock.Object, userManagerMock.Object, signInManagerMock.Object, passwordHasherMock.Object, getLoggedInUserMock.Object);

            // Set up GetRolesAsync method
            userManagerMock.Setup(um => um.GetRolesAsync(It.IsAny<Users>())).ReturnsAsync(new List<string> { "AccountHolder", "TellerPerson" });

            // Setup UserRepository to return the updated user
            userRepositoryMock.Setup(repo => repo.UpdateUsersAsync(It.IsAny<Users>()))
                .ReturnsAsync(new Users { Id = Id, UserName = "updatedUserName", Fullname = "Updated FullName", Address = "Updated Address", Email = "updatedemail@gmail.com" });

            // Act
            var result = await userService.UpdateUsersAsync(Id, userUpdateDTO);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Id, result.Id);
            Assert.Equal("Updated FullName", result.Fullname);
            Assert.Equal("updatedUserName", result.UserName);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnUserInfoDisplayDTO()
        {
            // Arrange
            var username = "existingUserName";
            var password = "correctPassword";

            var existingUser = new Users
            {
                UserName = username,
                PasswordHash = password
                // Set other properties as needed
            };

            var userRepositoryMock = new Mock<IUserRepository>();
            var mapperMock = new Mock<IMapper>();
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Users, UserInfoDisplayDTO>();
            });
            var mapper = mapperConfig.CreateMapper();

            var accountRepositoryMock = new Mock<IAccountRepository>();
            var configurationMock = new Mock<IConfiguration>();
            var userManagerMock = MockUserManager<Users>();
            var signInManagerMock = MockSignInManager<Users>();
            var passwordHasherMock = new Mock<IPasswordHasher<Users>>();
            var emailServiceMock = new Mock<IEmailService>();
            var accountServicesMock = new Mock<IAccountService>();
            var contextMock = new Mock<IHttpContextAccessor>();
            var getLoggedInUserMock = new Mock<GetLoggedinUser>(contextMock.Object);

            var userService = new UserService(userRepositoryMock.Object, mapper, accountServicesMock.Object, userManagerMock.Object, signInManagerMock.Object, passwordHasherMock.Object, getLoggedInUserMock.Object);

            // Set up GetRolesAsync method
            userManagerMock.Setup(um => um.GetRolesAsync(It.IsAny<Users>())).ReturnsAsync(new List<string> { "AccountHolder", "TellerPerson" });

            // Set up SignInManager to return Success when login is attempted
            signInManagerMock.Setup(sm => sm.PasswordSignInAsync(username, password, true, false))
                .ReturnsAsync(SignInResult.Success);

            // Set up UserManager to return the existing user
            userManagerMock.Setup(um => um.FindByNameAsync(username))
                .ReturnsAsync(existingUser);

            // Act
            var result = await userService.Login(username, password);

            // Assert
            Assert.NotNull(result);
        }

    }
}
