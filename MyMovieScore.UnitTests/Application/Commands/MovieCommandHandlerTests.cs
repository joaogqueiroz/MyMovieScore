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

        private const int Owner = 7;

        // A movie already saved in user 7's list
        private static Movie SavedMovie() =>
            new Movie("tt0111161", Owner, "The Shawshank Redemption", "Two imprisoned men bond.", "14 Oct 1994", "Drama", false, 0);

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
        public async Task CreateMovie_ImdbIdNotFoundOnOmdb_ReturnsNullAndSavesNothing()
        {
            var movieRepositoryMock = new Mock<IMovieRepository>();
            var externalServiceMock = new Mock<IIMDbExternalService>();
            externalServiceMock.Setup(s => s.GetByIMDbIdAsync("tt0000000")).ReturnsAsync((Movie?)null);
            var handler = new CreateMovieCommandHandler(movieRepositoryMock.Object, externalServiceMock.Object);

            var id = await handler.Handle(new CreateMovieCommand { UserId = 7, IdIMDb = "tt0000000", Watched = true, UserScore = 9 }, CancellationToken.None);

            Assert.Null(id);
            movieRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Movie>()), Times.Never);
        }

        [Fact]
        public async Task UpdateMovie_ChangesWatchedAndScore()
        {
            var movie = SavedMovie();
            var movieRepositoryMock = new Mock<IMovieRepository>();
            movieRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(movie);
            var handler = new UpdateMovieCommandHandler(movieRepositoryMock.Object);

            var found = await handler.Handle(new UpdateMovieCommand { Id = 1, UserId = Owner, Watched = true, UserScore = 8 }, CancellationToken.None);

            Assert.True(found);
            Assert.True(movie.Watched);
            Assert.Equal(8, movie.UserScore);
            movieRepositoryMock.Verify(r => r.UpdateAsync(movie), Times.Once);
        }

        [Fact]
        public async Task DeleteMovie_DeletesTheLoadedMovie()
        {
            var movie = SavedMovie();
            var movieRepositoryMock = new Mock<IMovieRepository>();
            movieRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(movie);
            var handler = new DeleteMovieCommandHandler(movieRepositoryMock.Object);

            var found = await handler.Handle(new DeleteMovieCommand(1, Owner), CancellationToken.None);

            Assert.True(found);
            movieRepositoryMock.Verify(r => r.DeleteAsync(movie), Times.Once);
        }

        [Fact]
        public async Task UpdateMovie_UnknownId_ReturnsFalseAndSavesNothing()
        {
            var movieRepositoryMock = new Mock<IMovieRepository>();
            var handler = new UpdateMovieCommandHandler(movieRepositoryMock.Object);

            var found = await handler.Handle(new UpdateMovieCommand { Id = 99, UserId = Owner, Watched = true, UserScore = 8 }, CancellationToken.None);

            Assert.False(found);
            movieRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Movie>()), Times.Never);
        }

        [Fact]
        public async Task DeleteMovie_UnknownId_ReturnsFalseAndDeletesNothing()
        {
            var movieRepositoryMock = new Mock<IMovieRepository>();
            var handler = new DeleteMovieCommandHandler(movieRepositoryMock.Object);

            var found = await handler.Handle(new DeleteMovieCommand(99, Owner), CancellationToken.None);

            Assert.False(found);
            movieRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Movie>()), Times.Never);
        }

        // Someone else's movie looks the same as a missing one: nothing is changed or revealed
        [Fact]
        public async Task UpdateMovie_SomeoneElsesMovie_ReturnsFalseAndSavesNothing()
        {
            var movie = SavedMovie();
            var movieRepositoryMock = new Mock<IMovieRepository>();
            movieRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(movie);
            var handler = new UpdateMovieCommandHandler(movieRepositoryMock.Object);

            var found = await handler.Handle(new UpdateMovieCommand { Id = 1, UserId = 8, Watched = true, UserScore = 1 }, CancellationToken.None);

            Assert.False(found);
            Assert.False(movie.Watched);
            Assert.Equal(0, movie.UserScore);
            movieRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Movie>()), Times.Never);
        }

        [Fact]
        public async Task DeleteMovie_SomeoneElsesMovie_ReturnsFalseAndDeletesNothing()
        {
            var movieRepositoryMock = new Mock<IMovieRepository>();
            movieRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(SavedMovie());
            var handler = new DeleteMovieCommandHandler(movieRepositoryMock.Object);

            var found = await handler.Handle(new DeleteMovieCommand(1, 8), CancellationToken.None);

            Assert.False(found);
            movieRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Movie>()), Times.Never);
        }

        [Fact]
        public async Task CreateMovie_SavesTheOmdbRatings()
        {
            var omdbMovie = OmdbMovie();
            omdbMovie.AddExternalRatings(new ExternalRatings("Internet Movie Database", "9.3/10"));
            omdbMovie.AddExternalRatings(new ExternalRatings("Rotten Tomatoes", "89%"));
            var movieRepositoryMock = new Mock<IMovieRepository>();
            var externalServiceMock = new Mock<IIMDbExternalService>();
            externalServiceMock.Setup(s => s.GetByIMDbIdAsync("tt0111161")).ReturnsAsync(omdbMovie);
            var handler = new CreateMovieCommandHandler(movieRepositoryMock.Object, externalServiceMock.Object);

            await handler.Handle(new CreateMovieCommand { UserId = 7, IdIMDb = "tt0111161", Watched = true, UserScore = 9 }, CancellationToken.None);

            movieRepositoryMock.Verify(r => r.AddAsync(It.Is<Movie>(m =>
                m.ExternalRatings.Count == 2 &&
                m.ExternalRatings[0].Source == "Internet Movie Database" && m.ExternalRatings[0].Value == "9.3/10" &&
                m.ExternalRatings[1].Source == "Rotten Tomatoes" && m.ExternalRatings[1].Value == "89%")), Times.Once);
        }
    }
}
