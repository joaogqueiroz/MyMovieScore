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
            // Searching user by email in DB
            var user = await _userRepository.GetByEmailAsync(request.Email);
            // if don't exists or the password doesn't match, login error
            if (user == null || !_authService.VerifyPassword(user.Password, request.Password))
            {
                return null;
            }
            // if exists, return token 
            var token = _authService.GenerateJwtToken(user.Id, user.Email);
            return new LoginUserViewModel(user.Email, token);
        }
    }
}
