using MediatR;
using MyMovieScore.Application.ViewModels;
using MyMovieScore.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMovieScore.Application.Queries.GetAllMovies
{
    public class GetAllMoviesQueryHandler : IRequestHandler<GetAllMoviesQuery, List<MovieViewModel>>
    {
        private readonly IMovieRepository _movieRepository;

        public GetAllMoviesQueryHandler(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }
        public async Task<List<MovieViewModel>> Handle(GetAllMoviesQuery request, CancellationToken cancellationToken)
        {
            var movies = await _movieRepository.GetAllAsync();
            var movieViewModel = movies
                .Select(m => new MovieViewModel
                (
                    m.Id,
                    m.IdIMDb,
                    m.UserId,
                    m.Name,
                    m.Description,
                    m.ReleaseDate,
                    m.Genre,
                    m.Watched,
                    m.UserScore))
                .ToList();
            return movieViewModel;
                
        }
    }
}
