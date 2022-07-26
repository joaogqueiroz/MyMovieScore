using MediatR;
using MyMovieScore.Application.ViewModels;
using MyMovieScore.Core.Repositories;
using MyMovieScore.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMovieScore.Application.Commands.LoginUser
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginUserViewModel>
    {
        private readonly IAuthService _authService;
        private readonly IUserRepository _userRepository;
        public LoginUserCommandHandler(IAuthService authService, IUserRepository userRepository)
        {
            _authService = authService;

            _userRepository = userRepository;
        }
        public async Task<LoginUserViewModel> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            // Using algorithm to create a hash password
            var passwordHash = _authService.ComputeSha256Hash(request.Password);
            // Searching user and hashpassword in DB 
            var user = await _userRepository.GetUserByLoginAndPasswordAsync(request.Email, passwordHash);
            // if don't exists, login error
            if (user == null)
            {
                return null;
            }
            // if exists, return token 
            var token = _authService.GenerateJwtToken(user.Email);
            return new LoginUserViewModel(user.Email, token);
        }
    }
}
