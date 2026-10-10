using FluentValidation.TestHelper;
using MyMovieScore.Application.Commands.CreateMovie;
using MyMovieScore.Application.Commands.UpdateMovie;
using MyMovieScore.Application.Validators;

namespace MyMovieScore.UnitTests.Application.Validators
{
    public class MovieCommandValidatorTests
    {
        private readonly CreateMovieCommandValidator _createValidator = new CreateMovieCommandValidator();
        private readonly UpdateMovieCommandValidator _updateValidator = new UpdateMovieCommandValidator();

        // UserId is not validated: the controller sets it from the access token
        private static CreateMovieCommand ValidCreateCommand() => new CreateMovieCommand
        {
            IdIMDb = "tt0111161",
            Watched = true,
            UserScore = 9.5f
        };

        [Fact]
        public void ValidCreateCommand_HasNoErrors()
        {
            var result = _createValidator.TestValidate(ValidCreateCommand());

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void CreateWithoutImdbId_HasImdbIdError(string? idIMDb)
        {
            var command = ValidCreateCommand();
            command.IdIMDb = idIMDb!;

            var result = _createValidator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(c => c.IdIMDb);
        }

        [Theory]
        [InlineData(-0.5f)]
        [InlineData(10.5f)]
        public void CreateWithScoreOutOfRange_HasScoreError(float score)
        {
            var command = ValidCreateCommand();
            command.UserScore = score;

            var result = _createValidator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(c => c.UserScore);
        }

        [Theory]
        [InlineData(0f)]
        [InlineData(10f)]
        public void CreateWithScoreOnTheLimits_IsValid(float score)
        {
            var command = ValidCreateCommand();
            command.UserScore = score;

            var result = _createValidator.TestValidate(command);

            result.ShouldNotHaveValidationErrorFor(c => c.UserScore);
        }

        [Fact]
        public void ValidUpdateCommand_HasNoErrors()
        {
            var result = _updateValidator.TestValidate(new UpdateMovieCommand { Id = 1, Watched = false, UserScore = 7 });

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void UpdateWithoutId_HasIdError()
        {
            var result = _updateValidator.TestValidate(new UpdateMovieCommand { Id = 0, Watched = true, UserScore = 7 });

            result.ShouldHaveValidationErrorFor(c => c.Id);
        }

        [Theory]
        [InlineData(-1f)]
        [InlineData(11f)]
        public void UpdateWithScoreOutOfRange_HasScoreError(float score)
        {
            var result = _updateValidator.TestValidate(new UpdateMovieCommand { Id = 1, Watched = true, UserScore = score });

            result.ShouldHaveValidationErrorFor(c => c.UserScore);
        }

        // Each rule has to answer with its own message, below and above the range,
        // instead of FluentValidation's default (which follows the server's language).
        [Theory]
        [InlineData(-1f)]
        [InlineData(10.5f)]
        public void ScoreOutOfRange_UsesTheScoreMessageOnBothEnds(float score)
        {
            var create = ValidCreateCommand();
            create.UserScore = score;

            _createValidator.TestValidate(create).ShouldHaveValidationErrorFor(c => c.UserScore).WithErrorMessage("Score must be between 0 and 10");
            _updateValidator.TestValidate(new UpdateMovieCommand { Id = 1, Watched = true, UserScore = score })
                .ShouldHaveValidationErrorFor(c => c.UserScore).WithErrorMessage("Score must be between 0 and 10");
        }

        [Fact]
        public void MissingIds_UseTheirOwnMessages()
        {
            var create = ValidCreateCommand();
            create.IdIMDb = "";

            var result = _createValidator.TestValidate(create);

            result.ShouldHaveValidationErrorFor(c => c.IdIMDb).WithErrorMessage("Should have IMDb ID");
            _updateValidator.TestValidate(new UpdateMovieCommand { Id = 0, Watched = true, UserScore = 5 })
                .ShouldHaveValidationErrorFor(c => c.Id).WithErrorMessage("Should have ID");
        }
    }
}
