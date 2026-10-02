using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyMovieScore.Core.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MyMovieScore.Infrastructure.Auth
{
    public class AuthService : IAuthService
    {
        private readonly JwtOptions _jwtOptions;
        private readonly PasswordHasher<object> _passwordHasher = new PasswordHasher<object>();

        public AuthService(IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
        }
        public string GenerateJwtToken(string email)
        {
            var securetyKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
            var credentials = new SigningCredentials(securetyKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
      {
          new Claim("userName", email),
       };
            var token = new JwtSecurityToken(
             issuer: _jwtOptions.Issuer,
             audience: _jwtOptions.Audience,
             expires: DateTime.UtcNow.AddHours(_jwtOptions.ExpirationHours),
             signingCredentials: credentials,
             claims: claims);

            var tokenHandler = new JwtSecurityTokenHandler();

            var stringToken = tokenHandler.WriteToken(token);
            return stringToken;
        }

        // PBKDF2 with a per-password salt. The default hasher does not use the user argument.
        public string HashPassword(string password)
        {
            return _passwordHasher.HashPassword(null!, password);
        }

        public bool VerifyPassword(string hashedPassword, string password)
        {
            return _passwordHasher.VerifyHashedPassword(null!, hashedPassword, password) != PasswordVerificationResult.Failed;
        }
    }
}
