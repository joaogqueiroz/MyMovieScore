using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMovieScore.Application.Commands.CreateMovie
{
    // Returns the new movie's id, or null when the IMDb id is not found on OMDb
    public class CreateMovieCommand : IRequest<int?>
    {
        public int UserId { get; set; }
        public string IdIMDb { get; set; }
        public bool Watched { get; set; }
        public float UserScore { get; set; }
    }
}
