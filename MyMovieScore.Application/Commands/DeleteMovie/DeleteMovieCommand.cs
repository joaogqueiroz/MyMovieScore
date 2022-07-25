using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMovieScore.Application.Commands.DeleteMovie
{
    public class DeleteMovieCommand : IRequest<Unit>
    {
        public int Id { get; set; }
    }
}
