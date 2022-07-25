using MediatR;
using MyMovieScore.Application.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMovieScore.Application.Queries.GetMovieById
{
    public class GetMovieByIdQuery : IRequest<MovieViewModel>
    {
        public GetMovieByIdQuery(int id)
        {
            Id = id;
        }

        public int Id { get; set; }
    }
}
