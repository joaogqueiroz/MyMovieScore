using MediatR;
using MyMovieScore.Core.Entities;
using MyMovieScore.Core.Repositories;
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
        public CreateMovieCommandHandler(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }
        public async Task<int> Handle(CreateMovieCommand request, CancellationToken cancellationToken)
        {
            var movie = new Movie(request.IdIMDb, request.UserId, "", "",DateTime.Now, "", request.Watched,request.UserScore);
            await _movieRepository.AddAsync(movie);
            return movie.Id;
        }
    }
}
