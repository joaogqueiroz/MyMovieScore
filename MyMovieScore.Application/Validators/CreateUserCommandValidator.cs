using FluentValidation;
using MyMovieScore.Application.Commands.CreateUser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyMovieScore.Application.Validators
{
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(p => p.Email)
            .EmailAddress()
            .WithMessage("Email format is wrong");

            RuleFor(p => p.Password)
                .Must(PasswordValidation)
                .WithMessage("Password must contain 8 characters, 1 uppercase, 1 lowercase and 1 special character");

            RuleFor(p => p.Name)
                  .NotEmpty()
                  .NotNull()
                  .WithMessage("Name cannot be null or empty");
        }
        public bool PasswordValidation(string password)
        {
            var regex = new Regex(@"^.*(?=.{8,})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!*@#$%^&+=]).*$");

            return regex.IsMatch(password);
        }
    }
}
