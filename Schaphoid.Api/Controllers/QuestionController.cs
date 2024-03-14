using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scaphoid.Core.Model;
using Scaphoid.Infrastructure.Data;

namespace Schaphoid.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QuestionController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public QuestionController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<object> Get(int page = 1)
        {
            int take = 20;
            int skip = (page - 1) * take;

            var questions = await _dbContext.Questions
                .Select(e => new
                {
                    e.Id,
                    e.Message,
                    e.Email,
                    e.CreatedOn
                })
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return questions;
        }

        [HttpPost]
        public async Task<IActionResult> Create(QuestionDto questionDto)
        {
            var question = new Question
            {
                CreatedOn = DateTime.Now,
                Email = questionDto.Email,
                Message = questionDto.Message,
            };

            _dbContext.Questions.Add(question);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }

    public class QuestionDto
    {
        public string Email { get; set; }
        public string Message { get; set; }
    }
}
