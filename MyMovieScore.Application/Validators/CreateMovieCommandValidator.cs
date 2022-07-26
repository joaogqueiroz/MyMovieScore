using FluentValidation;
using MyMovieScore.Application.Commands.CreateMovie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMovieScore.Application.Validators
{
    public class CreateMovieCommandValidator : AbstractValidator<CreateMovieCommand>
    {
        public CreateMovieCommandValidator()
        {
            RuleFor(m => m.UserId)
                .NotEmpty()
                .NotNull()
                .WithMessage("Should have user Id");
            
            RuleFor(m => m.IdIMDb)
                .NotEmpty()
                .NotNull()
                .WithMessage("Should have IMDb ID");

            RuleFor(m => m.Watched)
                .NotNull()
                .WithMessage("Should inform if was watched");

            RuleFor(m => m.UserScore)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(10)
                .WithMessage("Should hava a note between 0 to 10");

        }
    }
}
