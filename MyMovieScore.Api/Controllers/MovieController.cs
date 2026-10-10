using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyMovieScore.Application.Commands.CreateMovie;
using MyMovieScore.Application.Commands.DeleteMovie;
using MyMovieScore.Application.Commands.UpdateMovie;
using MyMovieScore.Application.Queries.GetAllMovies;
using MyMovieScore.Application.Queries.GetMovieById;
using System.Security.Claims;

namespace MyMovieScore.Api.Controllers
{
    [Route("api/movie")]
    [ApiController]
    [Authorize]
    public class MovieController : ControllerBase
    {
        private readonly IMediator _mediator;
        public MovieController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // The user always comes from the access token (the "sub" claim), never from the request
        private int? CurrentUserId =>
            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value, out var id) ? id : null;

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            if (CurrentUserId is not int userId) return Unauthorized();
            var query = new GetAllMoviesQuery(userId);
            var getAllMovies = await _mediator.Send(query);

            return Ok(getAllMovies);
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> GetById(int Id)
        {
            if (CurrentUserId is not int userId) return Unauthorized();
            var query = new GetMovieByIdQuery(Id, userId);
            var movie = await _mediator.Send(query);
            if (movie == null)
            {
                return NotFound();
            }
            return Ok(movie);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateMovieCommand command)
        {
            if (CurrentUserId is not int userId) return Unauthorized();
            command.UserId = userId;
            var id = await _mediator.Send(command);
            if (id == null)
            {
                return NotFound($"No movie with IMDb id '{command.IdIMDb}' was found on OMDb.");
            }
            return CreatedAtAction(nameof(GetById), new { id = id }, command);
        }
        [HttpPut]
        public async Task<IActionResult> Put(int id, [FromBody] UpdateMovieCommand command)
        {
            if (CurrentUserId is not int userId) return Unauthorized();
            command.UserId = userId;
            var found = await _mediator.Send(command);
            if (!found)
            {
                return NotFound();
            }
            return NoContent();
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(int Id)
        {
            if (CurrentUserId is not int userId) return Unauthorized();
            var command = new DeleteMovieCommand(Id, userId);
            var found = await _mediator.Send(command);
            if (!found)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
