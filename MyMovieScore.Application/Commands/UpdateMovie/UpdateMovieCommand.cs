using MediatR;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMovieScore.Application.Commands.UpdateMovie
{
    public class UpdateMovieCommand : IRequest<bool>
    {
        public int Id { get; set; }
        // Set from the access token; only the owner can change a movie
        [JsonIgnore]
        public int UserId { get; set; }
        public bool Watched { get; set; }
        public float UserScore { get; set; }
    }
}
