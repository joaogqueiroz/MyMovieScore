using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMovieScore.Core.Services
{
    public interface IAuthService
    {
        string GenerateJwtToken(int userId, string email);
        string HashPassword(string password);
        bool VerifyPassword(string hashedPassword, string password);
    }
}
