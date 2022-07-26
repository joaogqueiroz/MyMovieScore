using FluentValidation;
using MyMovieScore.Application.Commands.UpdateMovie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMovieScore.Application.Validators
{
    public class UpdateMovieCommandValidator : AbstractValidator<UpdateMovieCommand>
    {
        public UpdateMovieCommandValidator()
        {
            RuleFor(m => m.Id)
                .NotEmpty()
                .NotNull()
                .WithMessage("Should have ID");

            RuleFor(m => m.Watched)
                .NotEmpty()
                .NotNull()
                .WithMessage("Should inform if was watched");

            RuleFor(m => m.UserScore)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(10)
                .WithMessage("Should inform if was watched");
        }
    }
}
