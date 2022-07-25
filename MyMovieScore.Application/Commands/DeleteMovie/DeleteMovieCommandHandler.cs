using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMovieScore.Application.Commands.DeleteMovie
{
    public class DeleteMovieCommandHandler : IRequestHandler<DeleteMovieCommand, Unit>
    {
        public DeleteMovieCommandHandler()
        {

        }
        public async Task<Unit> Handle(DeleteMovieCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
