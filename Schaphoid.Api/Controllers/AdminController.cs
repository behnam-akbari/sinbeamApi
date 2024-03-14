using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scaphoid.Core.Model;
using Scaphoid.Infrastructure.Data;

namespace Schaphoid.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public AdminController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public Resource Get()
        {
            var resource = new Resource();

            resource.Links.Add(new Link("questions", Url.Action(nameof(Questions),
            null, null,
            Request.Scheme),
            HttpMethods.Get));

            resource.Links.Add(new Link("requests", Url.Action(nameof(Requests),
            null, null,
            Request.Scheme),
            HttpMethods.Get));

            return resource;
        }

        [HttpGet("[action]")]
        public async Task<object> Questions(int page = 1, int take = 20)
        {
            int skip = (page - 1) * take;

            var count = await _dbContext.Questions.CountAsync();

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

            return new
            {
                PageCount = ((count - 1) / take) + 1,
                Count = count,
                Items = questions
            };
        }

        [HttpGet("[action]")]
        public async Task<object> Requests(int page = 1, int take = 20)
        {
            int skip = (page - 1) * take;

            var count = await _dbContext.Requests.CountAsync();

            var requests = await _dbContext.Requests.Select(e => new
            {
                Id = e.OrderId,
                e.CreatedOn,
                e.Country.Name,
                e.Email,
                e.PhoneNumber
            })
            .Skip(skip)
            .Take(take)
            .ToListAsync();

            return new
            {
                PageCount = ((count - 1) / take) + 1,
                Count = count,
                Items = requests
            };
        }
    }
}
