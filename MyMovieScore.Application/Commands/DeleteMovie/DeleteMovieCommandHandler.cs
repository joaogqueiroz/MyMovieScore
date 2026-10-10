using MediatR;
using MyMovieScore.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMovieScore.Application.Commands.DeleteMovie
{
    public class DeleteMovieCommandHandler : IRequestHandler<DeleteMovieCommand, bool>
    {
        private readonly IMovieRepository _movieRepository;
        public DeleteMovieCommandHandler(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }
        public async Task<bool> Handle(DeleteMovieCommand request, CancellationToken cancellationToken)
        {
            var movie = await _movieRepository.GetByIdAsync(request.Id);
            // Someone else's movie is reported as not found, so its existence is not revealed
            if (movie == null || movie.UserId != request.UserId)
            {
                return false;
            }
            await _movieRepository.DeleteAsync(movie);
            return true;
        }
    }
}
