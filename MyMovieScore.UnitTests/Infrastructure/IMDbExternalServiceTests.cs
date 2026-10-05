using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using MyMovieScore.Infrastructure.ExternalServices;

namespace MyMovieScore.UnitTests.Infrastructure
{
    public class IMDbExternalServiceTests
    {
        private const string OmdbResponse = @"{
            ""Title"": ""The Shawshank Redemption"",
            ""Released"": ""14 Oct 1994"",
            ""Genre"": ""Drama"",
            ""Plot"": ""Two imprisoned men bond over a number of years."",
            ""Ratings"": [
                { ""Source"": ""Internet Movie Database"", ""Value"": ""9.3/10"" },
                { ""Source"": ""Rotten Tomatoes"", ""Value"": ""89%"" }
            ],
            ""imdbID"": ""tt0111161"",
            ""Response"": ""True""
        }";

        // Returns a canned response and remembers the request, so no real HTTP call is made.
        private class FakeHandler : HttpMessageHandler
        {
            private readonly string _body;
            private readonly HttpStatusCode _status;
            public HttpRequestMessage? Request { get; private set; }

            public FakeHandler(string body, HttpStatusCode status = HttpStatusCode.OK)
            {
                _body = body;
                _status = status;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Request = request;
                return Task.FromResult(new HttpResponseMessage(_status)
                {
                    Content = new StringContent(_body, Encoding.UTF8, "application/json")
                });
            }
        }

        private static IMDbExternalService CreateService(FakeHandler handler, string key) =>
            new IMDbExternalService(
                new HttpClient(handler) { BaseAddress = new Uri("https://www.omdbapi.com") },
                Options.Create(new ExternalServiceOptions { Key = key, Plot = "full" }));

        [Fact]
        public async Task GetByIMDbIdAsync_MapsTheOmdbResponse()
        {
            var movie = await CreateService(new FakeHandler(OmdbResponse), "test-key").GetByIMDbIdAsync("tt0111161");

            Assert.Equal("tt0111161", movie.IdIMDb);
            Assert.Equal("The Shawshank Redemption", movie.Name);
            Assert.Equal("Two imprisoned men bond over a number of years.", movie.Description);
            Assert.Equal("14 Oct 1994", movie.ReleaseDate);
            Assert.Equal("Drama", movie.Genre);
            Assert.Collection(movie.ExternalRatings,
                r => { Assert.Equal("Internet Movie Database", r.Source); Assert.Equal("9.3/10", r.Value); },
                r => { Assert.Equal("Rotten Tomatoes", r.Source); Assert.Equal("89%", r.Value); });
        }

        [Fact]
        public async Task GetByIMDbIdAsync_SendsIdKeyAndPlotToOmdb()
        {
            var handler = new FakeHandler(OmdbResponse);

            await CreateService(handler, "test-key").GetByIMDbIdAsync("tt0111161");

            var query = handler.Request!.RequestUri!.Query;
            Assert.Equal("www.omdbapi.com", handler.Request.RequestUri.Host);
            Assert.Contains("i=tt0111161", query);
            Assert.Contains("apikey=test-key", query);
            Assert.Contains("plot=full", query);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetByIMDbIdAsync_WithoutKey_ThrowsWithoutCallingOmdb(string key)
        {
            var handler = new FakeHandler(OmdbResponse);

            var error = await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService(handler, key).GetByIMDbIdAsync("tt0111161"));

            Assert.Contains("ExternalService:Key", error.Message);
            Assert.Null(handler.Request);
        }

        [Fact]
        public async Task GetByIMDbIdAsync_UnknownImdbId_ReturnsNull()
        {
            // What OMDb sends back with HTTP 200 for an id it does not know
            var handler = new FakeHandler(@"{ ""Response"": ""False"", ""Error"": ""Incorrect IMDb ID."" }");

            var movie = await CreateService(handler, "test-key").GetByIMDbIdAsync("tt0000000");

            Assert.Null(movie);
        }

        [Fact]
        public async Task GetByIMDbIdAsync_RejectedApiKey_Throws()
        {
            var handler = new FakeHandler(@"{ ""Response"": ""False"", ""Error"": ""Invalid API key!"" }", HttpStatusCode.Unauthorized);

            await Assert.ThrowsAsync<HttpRequestException>(() => CreateService(handler, "wrong-key").GetByIMDbIdAsync("tt0111161"));
        }

        [Fact]
        public async Task GetByIMDbIdAsync_MovieWithoutRatings_ReturnsMovieWithNoRatings()
        {
            var handler = new FakeHandler(@"{ ""Title"": ""Obscure film"", ""imdbID"": ""tt9999999"", ""Response"": ""True"" }");

            var movie = await CreateService(handler, "test-key").GetByIMDbIdAsync("tt9999999");

            Assert.NotNull(movie);
            Assert.Equal("Obscure film", movie!.Name);
            Assert.Empty(movie.ExternalRatings);
        }
    }
}
