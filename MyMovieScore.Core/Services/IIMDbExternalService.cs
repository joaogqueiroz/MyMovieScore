using MyMovieScore.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMovieScore.Core.Services
{
    public interface IIMDbExternalService
    {
        Task<Movie> GetByIMDbIdAsync(string idIMDb);

    }
}
