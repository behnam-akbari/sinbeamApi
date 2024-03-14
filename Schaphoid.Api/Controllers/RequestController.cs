using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scaphoid.Core.Model;
using Scaphoid.Infrastructure.Data;

namespace Schaphoid.Api.Controllers
{
    [ApiController]
    [Route("order/{orderId}/[controller]")]
    public class RequestController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public RequestController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> Create(int orderId, RequestDto requestDto)
        {
            var request = new Request
            {
                CreatedOn = DateTime.Now,
                Email = requestDto.Email,
                PhoneNumber = requestDto.PhoneNumber,
                CountryId = requestDto.CountryId,
                OrderId = orderId
            };

            _dbContext.Requests.Add(request);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }

    public class RequestDto
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int CountryId { get; set; }

    }
}
