using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMovieScore.Application.Commands.UpdateMovie
{
    public class UpdateMovieCommandHandler : IRequestHandler<UpdateMovieCommand, Unit>
    {
        public UpdateMovieCommandHandler()
        {

        }
        public async Task<Unit> Handle(UpdateMovieCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
