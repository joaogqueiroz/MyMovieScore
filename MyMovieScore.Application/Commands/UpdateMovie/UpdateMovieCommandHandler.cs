using MediatR;
using MyMovieScore.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMovieScore.Application.Commands.UpdateMovie
{
    public class UpdateMovieCommandHandler : IRequestHandler<UpdateMovieCommand, bool>
    {
        private readonly IMovieRepository _movieRepository;
        public UpdateMovieCommandHandler(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }
        public async Task<bool> Handle(UpdateMovieCommand request, CancellationToken cancellationToken)
        {
            var movie = await _movieRepository.GetByIdAsync(request.Id);
            // Someone else's movie is reported as not found, so its existence is not revealed
            if (movie == null || movie.UserId != request.UserId)
            {
                return false;
            }
            movie.Update(request.Watched, request.UserScore);
            await _movieRepository.UpdateAsync(movie);
            return true;
        }
    }
}
