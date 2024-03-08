using Microsoft.AspNetCore.Mvc;
using Scaphoid.Core.Model;
using Scaphoid.Infrastructure.Data;

namespace Schaphoid.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CountriesController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public CountriesController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public List<Country> Get()
        {
            var countries = _dbContext.Countries.ToList();

            return countries;
        }
    }
}
