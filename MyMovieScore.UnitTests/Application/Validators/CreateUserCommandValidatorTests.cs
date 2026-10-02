using FluentValidation.TestHelper;
using MyMovieScore.Application.Commands.CreateUser;
using MyMovieScore.Application.Validators;

namespace MyMovieScore.UnitTests.Application.Validators
{
    public class CreateUserCommandValidatorTests
    {
        private readonly CreateUserCommandValidator _validator = new CreateUserCommandValidator();

        private static CreateUserCommand ValidCommand() => new CreateUserCommand
        {
            Email = "user@test.com",
            Password = "Senha@123",
            Name = "Test User"
        };

        [Fact]
        public void ValidCommand_HasNoErrors()
        {
            var result = _validator.TestValidate(ValidCommand());

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("not-an-email")]
        [InlineData("user.test.com")]
        public void InvalidEmail_HasEmailError(string email)
        {
            var command = ValidCommand();
            command.Email = email;

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(c => c.Email);
        }

        [Theory]
        [InlineData("Se@1")]           // too short
        [InlineData("senha@123")]      // no uppercase
        [InlineData("SENHA@123")]      // no lowercase
        [InlineData("Senha@abc")]      // no digit
        [InlineData("Senha1234")]      // no special character
        public void WeakPassword_HasPasswordError(string password)
        {
            var command = ValidCommand();
            command.Password = password;

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(c => c.Password);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void MissingName_HasNameError(string? name)
        {
            var command = ValidCommand();
            command.Name = name!;

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(c => c.Name);
        }
    }
}
