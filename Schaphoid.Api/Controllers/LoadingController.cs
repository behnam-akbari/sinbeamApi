using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scaphoid.Core.Model;
using Scaphoid.Infrastructure.Data;
using Scaphoid.Infrastructure.Repositories;

namespace Schaphoid.Api.Controllers
{
    [ApiController]
    [Route("order/{orderId}/[controller]")]
    public class LoadingController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly WebSectionRepository _webSectionRepository;

        public LoadingController(ApplicationDbContext dbContext, WebSectionRepository webSectionRepository)
        {
            _webSectionRepository = webSectionRepository;
            _dbContext = dbContext;
        }

        [HttpGet]
        public ActionResult<LoadingDto> Get(int orderId)
        {
            var order = _dbContext.Orders.Where(e => e.Id == orderId)
                .Include(e => e.Localization)
                .Include(e => e.Loading)
                .ThenInclude(e => e.PointLoads)
                .FirstOrDefault();

            if (order is null)
            {
                return NotFound();
            }

            var section = _webSectionRepository.Get(order.Localization.SteelType, order.SectionId);

            var loading = order.Loading;

            LoadingDto loadingDto;

            if (loading is null)
            {
                loadingDto = new LoadingDto
                {
                    Span = order.Span,
                    SelfWeight = section.SelfWeight,
                    LoadType = LoadType.CharacteristicLoads,
                    DesignType = order.Localization.DesignType
                };
            }
            else
            {
                loadingDto = new LoadingDto
                {
                    Span = order.Span,
                    SelfWeight = section.SelfWeight,
                    LoadType = loading.LoadType,
                    DesignType = order.Localization.DesignType,
                    UltimatePointLoads = loading.LoadType == LoadType.UltimateLoads ?
                        loading.PointLoads.Select(e => new UltimatePointLoadDto
                        {
                            Id = e.Id,
                            Position = e.Position,
                            Load = e.Load
                        }).ToList() : null,
                    CharacteristicPointLoads = loading.LoadType == LoadType.CharacteristicLoads ?
                        loading.PointLoads.Select(e => new CharacteristicPointLoadDto
                        {
                            Id = e.Id,
                            Position = e.Position,
                            PermanentAction = e.PermanentAction,
                            VariableAction = e.VariableAction
                        }).ToList() : null,
                    PermanentLoads = loading.LoadType == LoadType.CharacteristicLoads ? loading.PermanentLoads : null,
                    VariableLoads = loading.LoadType == LoadType.CharacteristicLoads ? loading.VariableLoads : null,
                    UltimateLoads = loading.LoadType == LoadType.UltimateLoads ? loading.UltimateLoads : null,
                };
            }

            loadingDto.Links.Add(new Link("save-loading", Url.Action(nameof(Loading),
                null, new { orderId },
                Request.Scheme),
                HttpMethods.Post));

            return loadingDto;
        }

        [HttpPost]
        public IActionResult Loading(int orderId, LoadingDto loadingDto)
        {
            var order = _dbContext.Orders.Where(e => e.Id == orderId)
                .Include(e => e.Loading)
                .ThenInclude(e => e.PointLoads)
                .FirstOrDefault();

            if (order is null)
            {
                return NotFound();
            }

            if (order.Loading is null)
            {
                order.Loading = new Loading();
            }

            order.Loading.LoadType = loadingDto.LoadType;

            order.Loading.PointLoads?.Clear();

            order.Loading.PointLoads = new List<PointLoad>();

            if (loadingDto.LoadType == LoadType.CharacteristicLoads)
            {
                order.Loading.PermanentLoads = loadingDto.PermanentLoads;
                order.Loading.VariableLoads = loadingDto.VariableLoads;
                order.Loading.UltimateLoads = null;

                foreach (var pointLoadDto in loadingDto.CharacteristicPointLoads)
                {
                    order.Loading.PointLoads.Add(new PointLoad
                    {
                        Position = pointLoadDto.Position,
                        PermanentAction = pointLoadDto.PermanentAction,
                        VariableAction = pointLoadDto.VariableAction
                    });
                }
            }
            else
            {
                order.Loading.PermanentLoads = null;
                order.Loading.VariableLoads = null;
                order.Loading.UltimateLoads = loadingDto.UltimateLoads;

                foreach (var pointLoadDto in loadingDto.UltimatePointLoads)
                {
                    order.Loading.PointLoads.Add(new PointLoad
                    {
                        Position = pointLoadDto.Position,
                        Load = pointLoadDto.Load
                    });
                }
            }

            _dbContext.SaveChanges();

            return Ok();
        }
    }
}