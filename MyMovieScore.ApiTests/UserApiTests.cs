using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace MyMovieScore.ApiTests
{
    public class UserApiTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;
        private readonly HttpClient _client;

        public UserApiTests(ApiFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private static StringContent Json(string body) => new StringContent(body, Encoding.UTF8, "application/json");

        private static string NewEmail() => $"{Guid.NewGuid():N}@test.com";

        [Fact]
        public async Task CreateUser_ValidData_Returns201WithoutThePassword()
        {
            var response = await _client.PostAsJsonAsync("/api/user", new { email = NewEmail(), password = "Senha@123", name = "Test" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
            Assert.DoesNotContain("Senha@123", await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData(@"{ ""email"": 12345, ""password"": ""Senha@123"", ""name"": ""Test"" }")]     // number where text is expected
        [InlineData(@"{ ""email"": ""a@test.com"", ""password"": true, ""name"": ""Test"" }")]    // boolean where text is expected
        [InlineData(@"{ ""email"": ""a@test.com"", ""password"": ""Senha@123"", ""name"": [1] }")] // array where text is expected
        public async Task CreateUser_WrongJsonTypes_Returns400(string body)
        {
            var response = await _client.PostAsync("/api/user", Json(body));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData(@"{ ""email"": ""a@test.com"", ")]   // cut off
        [InlineData(@"not json at all")]
        [InlineData(@"")]
        public async Task CreateUser_MalformedOrEmptyBody_Returns400(string body)
        {
            var response = await _client.PostAsync("/api/user", Json(body));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateUser_InvalidFields_Returns400WithTheValidationMessages()
        {
            var response = await _client.PostAsJsonAsync("/api/user", new { email = "not-an-email", password = "weak", name = "" });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("Email format is wrong", body);
            Assert.Contains("Password must contain", body);
            Assert.Contains("Name cannot be null or empty", body);
        }

        [Fact]
        public async Task CreateUser_MissingPassword_Returns400InsteadOf500()
        {
            var response = await _client.PostAsJsonAsync("/api/user", new { email = NewEmail(), name = "Test" });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_WrongPassword_Returns400()
        {
            var email = NewEmail();
            await _client.PostAsJsonAsync("/api/user", new { email, password = "Senha@123", name = "Test" });

            var response = await _client.PostAsJsonAsync("/api/user/login", new { email, password = "Errada@123" });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_UnknownEmail_Returns400()
        {
            var response = await _client.PostAsJsonAsync("/api/user/login", new { email = NewEmail(), password = "Senha@123" });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Signs up a new user on its own client, logs in and returns their id with that client
        private async Task<(int UserId, HttpClient Client)> SignInAsync()
        {
            var client = _factory.CreateClient();
            var email = NewEmail();
            var created = await client.PostAsJsonAsync("/api/user", new { email, password = "Senha@123", name = "Test" });
            var userId = JsonDocument.Parse(await created.Content.ReadAsStringAsync()).RootElement.GetProperty("id").GetInt32();
            var login = await client.PostAsJsonAsync("/api/user/login", new { email, password = "Senha@123" });
            var token = JsonDocument.Parse(await login.Content.ReadAsStringAsync()).RootElement.GetProperty("token").GetString();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return (userId, client);
        }

        [Fact]
        public async Task GetUser_WithoutToken_Returns401()
        {
            var (userId, _) = await SignInAsync();

            Assert.Equal(HttpStatusCode.Unauthorized, (await _client.GetAsync($"/api/user/{userId}")).StatusCode);
        }

        [Fact]
        public async Task GetUser_Themselves_ReturnsNameAndEmail()
        {
            var (userId, client) = await SignInAsync();

            var response = await client.GetAsync($"/api/user/{userId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
            Assert.Equal(userId, user.GetProperty("id").GetInt32());
            Assert.EndsWith("@test.com", user.GetProperty("email").GetString());
        }

        // Another person's name and email are not shown; their id answers like a missing one
        [Fact]
        public async Task GetUser_SomeoneElse_Returns404()
        {
            var (otherUserId, _) = await SignInAsync();
            var (_, client) = await SignInAsync();

            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/user/{otherUserId}")).StatusCode);
        }

        [Fact]
        public async Task GetUser_UnknownId_Returns404()
        {
            var (_, client) = await SignInAsync();

            var response = await client.GetAsync("/api/user/999999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetUser_IdThatIsNotANumber_Returns400()
        {
            var (_, client) = await SignInAsync();

            var response = await client.GetAsync("/api/user/abc");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
