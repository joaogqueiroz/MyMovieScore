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
using Microsoft.Extensions.Configuration;

namespace MyMovieScore.Infrastructure.ExternalServices
{
    public class IMDbExternalService : IIMDbExternalService
    {
        private readonly IConfiguration _configuration;

        public IMDbExternalService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<Movie> GetByIMDbIdAsync(string idIMDb)
        {
            var value = _configuration.GetSection("ExternalService")["Key"];


            HttpClient client = new HttpClient { BaseAddress = new Uri("http://www.omdbapi.com") };
            var response = await client.GetAsync($"?i={idIMDb}&apikey={value}&plot=full");
            var content = await response.Content.ReadAsStringAsync();
            var movieDeserialize = JsonConvert.DeserializeObject<ImDbInforDto>(content);
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
            foreach (var item in movieDeserialize.Ratings)
            {
                ExternalRatings externalRatings = new ExternalRatings(item.Source, item.Value);
                movie.AddExternalRatings(externalRatings);
            }

            return movie;
        }
    }
}
