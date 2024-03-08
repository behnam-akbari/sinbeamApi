using Microsoft.AspNetCore.Mvc;
using Scaphoid.Core.Model;

namespace Schaphoid.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CountriesController : ControllerBase
    {
        [HttpPost]
        public List<Country> Get()
        {
            var countries = new List<Country>
            {
                new Country{ Id = 1, Name = "Iran" },
                new Country{ Id = 2, Name = "England"},
                new Country{ Id = 2, Name = "Ireland"}
            };

            return countries;
        }
    }
}
