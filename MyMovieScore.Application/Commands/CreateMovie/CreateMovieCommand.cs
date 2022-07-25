using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMovieScore.Application.Commands.CreateMovie
{
    public class CreateMovieCommand : IRequest<int>
    {
        public string IdIMDb { get; set; }
        public bool Watched { get; set; }
        public float UserScore { get; set; }
    }
}
