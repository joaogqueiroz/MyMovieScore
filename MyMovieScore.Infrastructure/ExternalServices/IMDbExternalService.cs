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

namespace MyMovieScore.Infrastructure.ExternalServices
{
    public class IMDbExternalService : IIMDbExternalService
    {
        public async Task<Movie> GetByIMDbIdAsync(string idIMDb)
        {
            HttpClient client = new HttpClient { BaseAddress = new Uri("http://www.omdbapi.com") };
            var response = await client.GetAsync($"?i={idIMDb}&apikey=1f936bec&plot=full");
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
