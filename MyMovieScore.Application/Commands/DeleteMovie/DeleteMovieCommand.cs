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
        public DeleteMovieCommand(int id, int userId)
        {
            Id = id;
            UserId = userId;
        }

        public int Id { get; set; }
        public int UserId { get; set; }
    }
}
