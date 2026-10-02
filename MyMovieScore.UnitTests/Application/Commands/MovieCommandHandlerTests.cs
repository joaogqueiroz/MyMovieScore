using Moq;
using MyMovieScore.Application.Commands.CreateMovie;
using MyMovieScore.Application.Commands.DeleteMovie;
using MyMovieScore.Application.Commands.UpdateMovie;
using MyMovieScore.Core.Entities;
using MyMovieScore.Core.Repositories;
using MyMovieScore.Core.Services;

namespace MyMovieScore.UnitTests.Application.Commands
{
    public class MovieCommandHandlerTests
    {
        private static Movie OmdbMovie() =>
            new Movie("tt0111161", 0, "The Shawshank Redemption", "Two imprisoned men bond.", "14 Oct 1994", "Drama", false, 0);

        [Fact]
        public async Task CreateMovie_UsesOmdbDataAndTheUsersChoices()
        {
            var movieRepositoryMock = new Mock<IMovieRepository>();
            var externalServiceMock = new Mock<IIMDbExternalService>();
            externalServiceMock.Setup(s => s.GetByIMDbIdAsync("tt0111161")).ReturnsAsync(OmdbMovie());
            var handler = new CreateMovieCommandHandler(movieRepositoryMock.Object, externalServiceMock.Object);

            await handler.Handle(new CreateMovieCommand { UserId = 7, IdIMDb = "tt0111161", Watched = true, UserScore = 9.5f }, CancellationToken.None);

            movieRepositoryMock.Verify(r => r.AddAsync(It.Is<Movie>(m =>
                m.IdIMDb == "tt0111161" &&
                m.Name == "The Shawshank Redemption" &&
                m.Description == "Two imprisoned men bond." &&
                m.ReleaseDate == "14 Oct 1994" &&
                m.Genre == "Drama" &&
                m.UserId == 7 &&
                m.Watched &&
                m.UserScore == 9.5f)), Times.Once);
        }

        [Fact]
        public async Task CreateMovie_WhenOmdbFails_DoesNotSave()
        {
            var movieRepositoryMock = new Mock<IMovieRepository>();
            var externalServiceMock = new Mock<IIMDbExternalService>();
            externalServiceMock.Setup(s => s.GetByIMDbIdAsync(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException("OMDb is down"));
            var handler = new CreateMovieCommandHandler(movieRepositoryMock.Object, externalServiceMock.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                handler.Handle(new CreateMovieCommand { UserId = 7, IdIMDb = "tt0111161", Watched = true, UserScore = 9 }, CancellationToken.None));

            movieRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Movie>()), Times.Never);
        }

        [Fact]
        public async Task UpdateMovie_ChangesWatchedAndScore()
        {
            var movie = OmdbMovie();
            var movieRepositoryMock = new Mock<IMovieRepository>();
            movieRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(movie);
            var handler = new UpdateMovieCommandHandler(movieRepositoryMock.Object);

            await handler.Handle(new UpdateMovieCommand { Id = 1, Watched = true, UserScore = 8 }, CancellationToken.None);

            Assert.True(movie.Watched);
            Assert.Equal(8, movie.UserScore);
            movieRepositoryMock.Verify(r => r.UpdateAsync(movie), Times.Once);
        }

        [Fact]
        public async Task DeleteMovie_DeletesTheLoadedMovie()
        {
            var movie = OmdbMovie();
            var movieRepositoryMock = new Mock<IMovieRepository>();
            movieRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(movie);
            var handler = new DeleteMovieCommandHandler(movieRepositoryMock.Object);

            await handler.Handle(new DeleteMovieCommand(1), CancellationToken.None);

            movieRepositoryMock.Verify(r => r.DeleteAsync(movie), Times.Once);
        }
    }
}
