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
            
            RuleFor(m => m.IdIMDb)
                .NotEmpty()
                .WithMessage("Should have IMDb ID");

            RuleFor(m => m.Watched)
                .NotNull()
                .WithMessage("Should inform if was watched");

            RuleFor(m => m.UserScore)
                .InclusiveBetween(0, 10)
                .WithMessage("Score must be between 0 and 10");

        }
    }
}
