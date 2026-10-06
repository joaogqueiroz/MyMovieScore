using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace MyMovieScore.ApiTests
{
    public class MovieApiTests : IClassFixture<ApiFactory>
    {
        private readonly HttpClient _client;

        public MovieApiTests(ApiFactory factory)
        {
            _client = factory.CreateClient();
        }

        private static StringContent Json(string body) => new StringContent(body, Encoding.UTF8, "application/json");

        // Signs up a new user, logs in and returns their id with an authorized client.
        private async Task<int> SignInAsync()
        {
            var email = $"{Guid.NewGuid():N}@test.com";
            var created = await _client.PostAsJsonAsync("/api/user", new { email, password = "Senha@123", name = "Test" });
            var userId = JsonDocument.Parse(await created.Content.ReadAsStringAsync()).RootElement.GetProperty("id").GetInt32();

            var login = await _client.PostAsJsonAsync("/api/user/login", new { email, password = "Senha@123" });
            var token = JsonDocument.Parse(await login.Content.ReadAsStringAsync()).RootElement.GetProperty("token").GetString();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return userId;
        }

        private async Task<int> AddMovieAsync(int userId)
        {
            var response = await _client.PostAsJsonAsync("/api/movie", new { userId, idIMDb = ApiFactory.KnownImdbId, watched = true, userScore = 9.5 });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return int.Parse(response.Headers.Location!.Segments.Last());
        }

        [Fact]
        public async Task Movies_WithoutToken_Return401()
        {
            Assert.Equal(HttpStatusCode.Unauthorized, (await _client.GetAsync("/api/movie")).StatusCode);
        }

        [Fact]
        public async Task Movies_WithAForgedToken_Return401()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/movie");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiJ9.eyJ1c2VyTmFtZSI6Im5vYm9keSJ9.forged");

            Assert.Equal(HttpStatusCode.Unauthorized, (await _client.SendAsync(request)).StatusCode);
        }

        [Fact]
        public async Task AddMovie_ValidData_CanBeReadBack()
        {
            var userId = await SignInAsync();

            var id = await AddMovieAsync(userId);

            var movie = JsonDocument.Parse(await _client.GetStringAsync($"/api/movie/{id}")).RootElement;
            Assert.Equal("The Shawshank Redemption", movie.GetProperty("name").GetString());
            Assert.Equal(9.5, movie.GetProperty("userScore").GetDouble());
            Assert.Equal(userId, movie.GetProperty("userId").GetInt32());
        }

        [Fact]
        public async Task AddMovie_ImdbIdNotOnOmdb_Returns404()
        {
            var userId = await SignInAsync();

            var response = await _client.PostAsJsonAsync("/api/movie", new { userId, idIMDb = "tt0000000", watched = false, userScore = 5 });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData(@"{ ""userId"": 1, ""idIMDb"": ""tt0111161"", ""watched"": true, ""userScore"": ""nine"" }")] // text in a number
        [InlineData(@"{ ""userId"": ""one"", ""idIMDb"": ""tt0111161"", ""watched"": true, ""userScore"": 9 }")]  // text in an id
        [InlineData(@"{ ""userId"": 1, ""idIMDb"": ""tt0111161"", ""watched"": ""yes"", ""userScore"": 9 }")]   // text in a boolean
        [InlineData(@"{ ""userId"": 1.5, ""idIMDb"": ""tt0111161"", ""watched"": true, ""userScore"": 9 }")]    // decimal in an integer
        public async Task AddMovie_WrongJsonTypes_Returns400(string body)
        {
            await SignInAsync();

            var response = await _client.PostAsync("/api/movie", Json(body));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(10.5)]
        public async Task AddMovie_ScoreOutOfRange_Returns400(double score)
        {
            var userId = await SignInAsync();

            var response = await _client.PostAsJsonAsync("/api/movie", new { userId, idIMDb = ApiFactory.KnownImdbId, watched = true, userScore = score });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetMovie_UnknownId_Returns404()
        {
            await SignInAsync();

            Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/movie/999999")).StatusCode);
        }

        [Fact]
        public async Task UpdateMovie_UnknownId_Returns404()
        {
            await SignInAsync();

            var response = await _client.PutAsJsonAsync("/api/movie", new { id = 999999, watched = true, userScore = 7 });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateMovie_ScoreIsText_Returns400()
        {
            await SignInAsync();

            var response = await _client.PutAsync("/api/movie", Json(@"{ ""id"": 1, ""watched"": true, ""userScore"": ""seven"" }"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateMovie_ExistingMovie_ChangesWatchedAndScore()
        {
            var userId = await SignInAsync();
            var id = await AddMovieAsync(userId);

            var response = await _client.PutAsJsonAsync("/api/movie", new { id, watched = false, userScore = 6.5 });

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var movie = JsonDocument.Parse(await _client.GetStringAsync($"/api/movie/{id}")).RootElement;
            Assert.False(movie.GetProperty("watched").GetBoolean());
            Assert.Equal(6.5, movie.GetProperty("userScore").GetDouble());
        }

        [Fact]
        public async Task DeleteMovie_UnknownId_Returns404()
        {
            await SignInAsync();

            Assert.Equal(HttpStatusCode.NotFound, (await _client.DeleteAsync("/api/movie?id=999999")).StatusCode);
        }

        // The movie has ratings, so this also proves they are deleted with it (cascade on the foreign key)
        [Fact]
        public async Task DeleteMovie_ExistingMovie_IsGoneAfterwards()
        {
            var userId = await SignInAsync();
            var id = await AddMovieAsync(userId);

            Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/movie?id={id}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/movie/{id}")).StatusCode);
        }

        [Fact]
        public async Task AddMovie_SavesTheOmdbRatings()
        {
            var userId = await SignInAsync();
            var id = await AddMovieAsync(userId);

            var ratings = JsonDocument.Parse(await _client.GetStringAsync($"/api/movie/{id}")).RootElement
                .GetProperty("externalRatings").EnumerateArray()
                .Select(r => $"{r.GetProperty("source").GetString()}={r.GetProperty("value").GetString()}")
                .OrderBy(r => r);

            Assert.Equal(new[] { "Internet Movie Database=9.3/10", "Metacritic=82/100", "Rotten Tomatoes=89%" }, ratings);
        }

        [Fact]
        public async Task GetAllMovies_IncludesTheRatings()
        {
            var userId = await SignInAsync();
            var id = await AddMovieAsync(userId);

            var movie = JsonDocument.Parse(await _client.GetStringAsync("/api/movie")).RootElement
                .EnumerateArray().Single(m => m.GetProperty("id").GetInt32() == id);

            Assert.Equal(3, movie.GetProperty("externalRatings").GetArrayLength());
        }

        [Fact]
        public async Task UpdateMovie_KeepsTheRatings()
        {
            var userId = await SignInAsync();
            var id = await AddMovieAsync(userId);

            await _client.PutAsJsonAsync("/api/movie", new { id, watched = false, userScore = 7 });

            var movie = JsonDocument.Parse(await _client.GetStringAsync($"/api/movie/{id}")).RootElement;
            Assert.Equal(3, movie.GetProperty("externalRatings").GetArrayLength());
        }
    }
}
