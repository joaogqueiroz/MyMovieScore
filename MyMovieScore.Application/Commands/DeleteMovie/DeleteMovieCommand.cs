using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMovieScore.Application.Commands.DeleteMovie
{
    public class DeleteMovieCommand : IRequest<bool>
    {
        public DeleteMovieCommand(int id)
        {
            Id = id;
        }

        public int Id { get; set; }
    }
}
