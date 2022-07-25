using MediatR;
using MyMovieScore.Application.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMovieScore.Application.Queries.GetAllMovies
{
    public class GetAllMoviesQueryHandler : IRequestHandler<GetAllMoviesQuery, List<MovieViewModel>>
    {
        public GetAllMoviesQueryHandler()
        {

        }
        public Task<List<MovieViewModel>> Handle(GetAllMoviesQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
