using Moq;
using MyMovieScore.Application.Queries.GetAllMovies;
using MyMovieScore.Application.Queries.GetMovieById;
using MyMovieScore.Application.Queries.GetUserById;
using MyMovieScore.Core.Entities;
using MyMovieScore.Core.Repositories;

namespace MyMovieScore.UnitTests.Application.Queries
{
    public class QueryHandlerTests
    {
        private static Movie Movie(string idIMDb, string name) =>
            new Movie(idIMDb, 3, name, "Plot", "1994", "Drama", true, 9);

        [Fact]
        public async Task GetAllMovies_MapsEveryMovie()
        {
            var movieRepositoryMock = new Mock<IMovieRepository>();
            movieRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Movie>
            {
                Movie("tt0111161", "The Shawshank Redemption"),
                Movie("tt0068646", "The Godfather")
            });
            var handler = new GetAllMoviesQueryHandler(movieRepositoryMock.Object);

            var result = await handler.Handle(new GetAllMoviesQuery(), CancellationToken.None);

            Assert.Equal(2, result.Count);
            Assert.Equal("The Shawshank Redemption", result[0].Name);
            Assert.Equal("tt0068646", result[1].IdIMDb);
        }

        [Fact]
        public async Task GetMovieById_Found_ReturnsViewModel()
        {
            var movieRepositoryMock = new Mock<IMovieRepository>();
            movieRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(Movie("tt0111161", "The Shawshank Redemption"));
            var handler = new GetMovieByIdQueryHandler(movieRepositoryMock.Object);

            var result = await handler.Handle(new GetMovieByIdQuery(1), CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("The Shawshank Redemption", result.Name);
            Assert.Equal(3, result.UserId);
            Assert.True(result.Watched);
            Assert.Equal(9, result.UserScore);
        }

        [Fact]
        public async Task GetMovieById_NotFound_ReturnsNull()
        {
            var handler = new GetMovieByIdQueryHandler(new Mock<IMovieRepository>().Object);

            var result = await handler.Handle(new GetMovieByIdQuery(99), CancellationToken.None);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserById_Found_ReturnsNameAndEmailOnly()
        {
            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new User("user@test.com", "hashed-password", "Test"));
            var handler = new GetUserByIdQueryHandler(userRepositoryMock.Object);

            var result = await handler.Handle(new GetUserByIdQuery(1), CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("Test", result.Name);
            Assert.Equal("user@test.com", result.Email);
            Assert.Null(typeof(MyMovieScore.Application.ViewModels.UserViewModel).GetProperty("Password"));
        }

        [Fact]
        public async Task GetUserById_NotFound_ReturnsNull()
        {
            var handler = new GetUserByIdQueryHandler(new Mock<IUserRepository>().Object);

            var result = await handler.Handle(new GetUserByIdQuery(99), CancellationToken.None);

            Assert.Null(result);
        }
    }
}
