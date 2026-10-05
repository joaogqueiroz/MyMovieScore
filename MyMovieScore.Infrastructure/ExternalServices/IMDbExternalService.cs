using MyMovieScore.Core.Entities;
using MyMovieScore.Core.Services;
using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MyMovieScore.Core.DTOs;
using Microsoft.Extensions.Options;

namespace MyMovieScore.Infrastructure.ExternalServices
{
    public class IMDbExternalService : IIMDbExternalService
    {
        private readonly HttpClient _httpClient;
        private readonly ExternalServiceOptions _options;

        // HttpClient comes from IHttpClientFactory, with BaseAddress set from ExternalService:BaseUrl
        public IMDbExternalService(HttpClient httpClient, IOptions<ExternalServiceOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<Movie?> GetByIMDbIdAsync(string idIMDb)
        {
            if (string.IsNullOrWhiteSpace(_options.Key))
            {
                throw new InvalidOperationException("The OMDb API key is not configured. Set ExternalService:Key.");
            }

            var response = await _httpClient.GetAsync($"?i={idIMDb}&apikey={_options.Key}&plot={_options.Plot}");
            // OMDb answers 401 for a bad key: a server configuration problem, not a missing movie
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var movieDeserialize = JsonConvert.DeserializeObject<ImDbInforDto>(content);
            if (movieDeserialize?.Response != "True")
            {
                return null;
            }
            var movie = new Movie(
                movieDeserialize.ImdbId,
                0,
                movieDeserialize.Title,
                movieDeserialize.Plot,
                movieDeserialize.Released,
                movieDeserialize.Genre,
                false,
                0
                );
            foreach (var item in movieDeserialize.Ratings ?? new List<Rating>())
            {
                ExternalRatings externalRatings = new ExternalRatings(item.Source, item.Value);
                movie.AddExternalRatings(externalRatings);
            }

            return movie;
        }
    }
}
