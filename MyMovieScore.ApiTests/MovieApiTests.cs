using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyMovieScore.Infrastructure.Auth;

namespace MyMovieScore.ApiTests
{
    public class MovieApiTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;
        private readonly HttpClient _client;

        public MovieApiTests(ApiFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private static StringContent Json(string body) => new StringContent(body, Encoding.UTF8, "application/json");

        // Signs up a new user, logs in and returns their id with an authorized client.
        private Task<int> SignInAsync() => SignInAsync(_client);

        private static async Task<int> SignInAsync(HttpClient client)
        {
            var email = $"{Guid.NewGuid():N}@test.com";
            var created = await client.PostAsJsonAsync("/api/user", new { email, password = "Senha@123", name = "Test" });
            var userId = JsonDocument.Parse(await created.Content.ReadAsStringAsync()).RootElement.GetProperty("id").GetInt32();

            var login = await client.PostAsJsonAsync("/api/user/login", new { email, password = "Senha@123" });
            var token = JsonDocument.Parse(await login.Content.ReadAsStringAsync()).RootElement.GetProperty("token").GetString();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return userId;
        }

        // A second, separately signed-in user
        private async Task<(int UserId, HttpClient Client)> OtherUserAsync()
        {
            var client = _factory.CreateClient();
            return (await SignInAsync(client), client);
        }

        private async Task<int> AddMovieAsync(int userId)
        {
            var response = await _client.PostAsJsonAsync("/api/movie", new { idIMDb = ApiFactory.KnownImdbId, watched = true, userScore = 9.5 });
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

            var response = await _client.PostAsJsonAsync("/api/movie", new { idIMDb = "tt0000000", watched = false, userScore = 5 });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData(@"{ ""userId"": 1, ""idIMDb"": ""tt0111161"", ""watched"": true, ""userScore"": ""nine"" }")] // text in a number
        [InlineData(@"{ ""idIMDb"": 111161, ""watched"": true, ""userScore"": 9 }")]                     // number in the IMDb id
        [InlineData(@"{ ""userId"": 1, ""idIMDb"": ""tt0111161"", ""watched"": ""yes"", ""userScore"": 9 }")]   // text in a boolean
        [InlineData(@"{ ""idIMDb"": ""tt0111161"", ""watched"": true, ""userScore"": [9] }")]            // array in a number
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

            var response = await _client.PostAsJsonAsync("/api/movie", new { idIMDb = ApiFactory.KnownImdbId, watched = true, userScore = score });

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

        // The movie goes to the signed-in user's list, whatever userId the body claims
        [Fact]
        public async Task AddMovie_UserIdInTheBody_IsIgnored()
        {
            var (otherUserId, _) = await OtherUserAsync();
            var userId = await SignInAsync();

            var response = await _client.PostAsJsonAsync("/api/movie", new { userId = otherUserId, idIMDb = ApiFactory.KnownImdbId, watched = true, userScore = 8 });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var id = int.Parse(response.Headers.Location!.Segments.Last());
            var movie = JsonDocument.Parse(await _client.GetStringAsync($"/api/movie/{id}")).RootElement;
            Assert.Equal(userId, movie.GetProperty("userId").GetInt32());
        }

        [Fact]
        public async Task GetAllMovies_ReturnsOnlyTheUsersOwnMovies()
        {
            var userId = await SignInAsync();
            var mine = await AddMovieAsync(userId);
            var (_, other) = await OtherUserAsync();

            var othersList = JsonDocument.Parse(await other.GetStringAsync("/api/movie")).RootElement;
            var myList = JsonDocument.Parse(await _client.GetStringAsync("/api/movie")).RootElement;

            Assert.Equal(0, othersList.GetArrayLength());
            Assert.All(myList.EnumerateArray(), m => Assert.Equal(userId, m.GetProperty("userId").GetInt32()));
            Assert.Contains(myList.EnumerateArray(), m => m.GetProperty("id").GetInt32() == mine);
        }

        // Someone else's movie answers like a missing one, and stays unchanged
        [Fact]
        public async Task SomeoneElsesMovie_CannotBeReadChangedOrDeleted()
        {
            var userId = await SignInAsync();
            var id = await AddMovieAsync(userId);
            var (_, other) = await OtherUserAsync();

            Assert.Equal(HttpStatusCode.NotFound, (await other.GetAsync($"/api/movie/{id}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await other.PutAsJsonAsync("/api/movie", new { id, watched = false, userScore = 1 })).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await other.DeleteAsync($"/api/movie?id={id}")).StatusCode);

            var movie = JsonDocument.Parse(await _client.GetStringAsync($"/api/movie/{id}")).RootElement;
            Assert.True(movie.GetProperty("watched").GetBoolean());
            Assert.Equal(9.5, movie.GetProperty("userScore").GetDouble());
        }

        // A validly signed token without the user id (the format issued before) must sign in again
        [Fact]
        public async Task TokenWithoutUserId_Returns401()
        {
            var jwt = _factory.Services.GetRequiredService<IOptions<JwtOptions>>().Value;
            var token = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
                issuer: jwt.Issuer,
                audience: jwt.Audience,
                claims: new[] { new System.Security.Claims.Claim("userName", "someone@test.com") },
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)), SecurityAlgorithms.HmacSha256)));
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/movie");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            Assert.Equal(HttpStatusCode.Unauthorized, (await _client.SendAsync(request)).StatusCode);
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
