using Moq;
using MyMovieScore.Application.Commands.CreateUser;
using MyMovieScore.Application.Commands.LoginUser;
using MyMovieScore.Core.Entities;
using MyMovieScore.Core.Repositories;
using MyMovieScore.Core.Services;

namespace MyMovieScore.UnitTests.Application.Commands
{
    public class UserCommandHandlerTests
    {
        private const string Email = "user@test.com";
        private const string HashedPassword = "hashed-password";

        [Fact]
        public async Task CreateUser_SavesUserWithHashedPassword()
        {
            var userRepositoryMock = new Mock<IUserRepository>();
            var authServiceMock = new Mock<IAuthService>();
            authServiceMock.Setup(a => a.HashPassword("Senha@123")).Returns(HashedPassword);
            var handler = new CreateUserCommandHandler(userRepositoryMock.Object, authServiceMock.Object);

            await handler.Handle(new CreateUserCommand { Email = Email, Password = "Senha@123", Name = "Test" }, CancellationToken.None);

            userRepositoryMock.Verify(r => r.AddAsync(It.Is<User>(u =>
                u.Email == Email &&
                u.Name == "Test" &&
                u.Password == HashedPassword)), Times.Once);
        }

        private static LoginUserCommandHandler LoginHandler(bool passwordMatches, out Mock<IAuthService> authServiceMock)
        {
            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(r => r.GetByEmailAsync(Email)).ReturnsAsync(new User(Email, HashedPassword, "Test"));

            authServiceMock = new Mock<IAuthService>();
            authServiceMock.Setup(a => a.VerifyPassword(HashedPassword, It.IsAny<string>())).Returns(passwordMatches);
            authServiceMock.Setup(a => a.GenerateJwtToken(Email)).Returns("token");

            return new LoginUserCommandHandler(authServiceMock.Object, userRepositoryMock.Object);
        }

        [Fact]
        public async Task Login_RightPassword_ReturnsToken()
        {
            var handler = LoginHandler(passwordMatches: true, out var authServiceMock);

            var result = await handler.Handle(new LoginUserCommand { Email = Email, Password = "Senha@123" }, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(Email, result.Email);
            Assert.Equal("token", result.Token);
            authServiceMock.Verify(a => a.VerifyPassword(HashedPassword, "Senha@123"), Times.Once);
        }

        [Fact]
        public async Task Login_WrongPassword_ReturnsNull()
        {
            var handler = LoginHandler(passwordMatches: false, out var authServiceMock);

            var result = await handler.Handle(new LoginUserCommand { Email = Email, Password = "wrong" }, CancellationToken.None);

            Assert.Null(result);
            authServiceMock.Verify(a => a.GenerateJwtToken(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Login_UnknownEmail_ReturnsNull()
        {
            var handler = LoginHandler(passwordMatches: true, out var authServiceMock);

            var result = await handler.Handle(new LoginUserCommand { Email = "nobody@test.com", Password = "Senha@123" }, CancellationToken.None);

            Assert.Null(result);
            authServiceMock.Verify(a => a.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
