using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMovieScore.Application.Commands.UpdateMovie
{
    public class UpdateMovieCommand : IRequest<Unit>
    {
        public int Id { get; set; }
        public bool Watched { get; set; }
        public float UserScore { get; set; }
    }
}
