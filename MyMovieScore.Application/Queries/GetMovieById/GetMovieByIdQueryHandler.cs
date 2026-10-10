using MediatR;
using MyMovieScore.Application.ViewModels;
using MyMovieScore.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMovieScore.Application.Queries.GetMovieById
{
    public class GetMovieByIdQueryHandler : IRequestHandler<GetMovieByIdQuery, MovieViewModel>
    {
        private readonly IMovieRepository _movieRepository;

        public GetMovieByIdQueryHandler(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }

        public async Task<MovieViewModel> Handle(GetMovieByIdQuery request, CancellationToken cancellationToken)
        {
            var movie = await _movieRepository.GetByIdAsync(request.Id);
            // Someone else's movie is reported as not found, so its existence is not revealed
            if (movie == null || movie.UserId != request.UserId) return null;
            var movieViewModel = new MovieViewModel
                (
                movie.Id,
                movie.IdIMDb,
                movie.UserId,
                movie.Name,
                movie.Description,
                movie.ReleaseDate,
                movie.Genre,
                movie.Watched,
                movie.UserScore,
                movie.ExternalRatings.Select(r => new ExternalRatingViewModel(r.Source, r.Value)).ToList()
                );
            return movieViewModel;
        }
    }
}
