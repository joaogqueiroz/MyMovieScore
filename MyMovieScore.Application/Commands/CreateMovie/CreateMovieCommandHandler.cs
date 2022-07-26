using MediatR;
using MyMovieScore.Core.Entities;
using MyMovieScore.Core.Repositories;
using MyMovieScore.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMovieScore.Application.Commands.CreateMovie
{
    public class CreateMovieCommandHandler : IRequestHandler<CreateMovieCommand, int>
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IIMDbExternalService _iIMDbExternalService;
        public CreateMovieCommandHandler(IMovieRepository movieRepository, IIMDbExternalService iIMDbExternalService)
        {
            _movieRepository = movieRepository;
            _iIMDbExternalService = iIMDbExternalService;
        }
        public async Task<int> Handle(CreateMovieCommand request, CancellationToken cancellationToken)
        {
            var externalMovie = await _iIMDbExternalService.GetByIMDbIdAsync(request.IdIMDb);
            var movie = new Movie(externalMovie.IdIMDb, request.UserId, externalMovie.Name,externalMovie.Description,externalMovie.ReleaseDate,externalMovie.Genre,request.Watched,request.UserScore);
            await _movieRepository.AddAsync(movie);
            return movie.Id;
        }
    }
}
