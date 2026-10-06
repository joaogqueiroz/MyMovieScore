using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyMovieScore.Core.Entities;
using MyMovieScore.Core.Services;
using Testcontainers.MsSql;

namespace MyMovieScore.ApiTests
{
    // Runs the real API in memory against SQL Server in a container (migrations applied on
    // startup, as in production). Only the OMDb client is replaced, so no internet is needed.
    public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        public const string KnownImdbId = "tt0111161";

        private readonly MsSqlContainer _sqlServer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();

        public async Task InitializeAsync() => await _sqlServer.StartAsync();

        public new async Task DisposeAsync()
        {
            await base.DisposeAsync();
            await _sqlServer.DisposeAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var connectionString = new SqlConnectionStringBuilder(_sqlServer.GetConnectionString())
            {
                InitialCatalog = "MyMovieScoreApiTests"
            }.ConnectionString;

            builder.UseSetting("ConnectionStrings:MyMovieScoreCs", connectionString);
            builder.UseSetting("Database:ApplyMigrationsOnStartup", "true");
            builder.UseSetting("ExternalService:Key", "test-key");

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IIMDbExternalService>();
                services.AddScoped<IIMDbExternalService, FakeOmdb>();
            });
        }

        private class FakeOmdb : IIMDbExternalService
        {
            public Task<Movie?> GetByIMDbIdAsync(string idIMDb)
            {
                if (idIMDb != KnownImdbId)
                {
                    return Task.FromResult<Movie?>(null);
                }

                var movie = new Movie(KnownImdbId, 0, "The Shawshank Redemption", "Two imprisoned men bond.", "14 Oct 1994", "Drama", false, 0);
                movie.AddExternalRatings(new ExternalRatings("Internet Movie Database", "9.3/10"));
                movie.AddExternalRatings(new ExternalRatings("Rotten Tomatoes", "89%"));
                movie.AddExternalRatings(new ExternalRatings("Metacritic", "82/100"));
                return Task.FromResult<Movie?>(movie);
            }
        }
    }
}
