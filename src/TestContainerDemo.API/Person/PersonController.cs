using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestContainerDemo.API.Models;
using TestContainerDemo.Common;

namespace TestContainerDemo.API.Person
{
    [ApiController]
    [Route("[controller]")]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public sealed class PersonController : ControllerBase
    {
        private readonly ILogger<PersonController> _logger;
        private readonly ApplicationDbContext _dbContext;

        public PersonController(ILogger<PersonController> logger, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet(Name = "GetPeople")]
        [ProducesResponseType(typeof(IEnumerable<PersonResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get()
        {
            try
            {
                var people = await _dbContext.People.ToListAsync();
                return Ok(people.Select(x => new PersonResponse
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Occupation = x.Occupation
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get people failed");
                return Problem("An error occured");
            }
        }

        [HttpGet("{id}", Name = nameof(GetPerson))]
        [ActionName(nameof(GetPerson))]
        [ProducesResponseType(typeof(PersonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPerson(int id)
        {
            try
            {
                var person = await _dbContext.People.FindAsync(id);

                if(person is null)
                {
                    return NotFound();
                }

                return Ok(new PersonResponse
                {
                    Id = person.Id,
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    Occupation = person.Occupation
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get person {id} failed.", id);
                return Problem("An error occured");
            }
        }

        [HttpPost(Name = nameof(CreatePerson))]
        [ProducesResponseType(typeof(PersonResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePerson(CreatePersonRequest request)
        {
            try
            {
                request.Validate();

                var person = new Models.Person
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Occupation = request.Occupation
                };

                _dbContext.People.Add(person);

                await _dbContext.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPerson), new { id = person.Id }, null);
            }
            catch(ValidationException vx)
            {
                return Problem(vx.Message, statusCode: StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create person failed.");
                return Problem("An error occured");
            }
        }
    }
}