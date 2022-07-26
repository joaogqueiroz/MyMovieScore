using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyMovieScore.Application.Commands.CreateMovie;
using MyMovieScore.Application.Commands.DeleteMovie;
using MyMovieScore.Application.Commands.UpdateMovie;
using MyMovieScore.Application.Queries.GetAllMovies;
using MyMovieScore.Application.Queries.GetMovieById;

namespace MyMovieScore.Api.Controllers
{
    [Route("api/movie")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly IMediator _mediator;
        public MovieController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var query = new GetAllMoviesQuery();
            var getAllMovies = await _mediator.Send(query);

            return Ok(getAllMovies);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int Id)
        {
            var query = new GetMovieByIdQuery(Id);
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
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = id }, command);
        }
        [HttpPut]
        public async Task<IActionResult> Put(int id, [FromBody] UpdateMovieCommand command)
        {
            await _mediator.Send(command);
            return NoContent();
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(int Id)
        {
            var command = new DeleteMovieCommand(Id);
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
