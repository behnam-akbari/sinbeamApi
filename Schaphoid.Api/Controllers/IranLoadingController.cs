using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scaphoid.Core.Model;
using Scaphoid.Infrastructure.Data;
using Scaphoid.Infrastructure.Repositories;

namespace Schaphoid.Api.Controllers
{
    [ApiController]
    [Route("order/{orderId}/[controller]")]
    public class IranLoadingController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly WebSectionRepository _webSectionRepository;

        public IranLoadingController(ApplicationDbContext dbContext, WebSectionRepository webSectionRepository)
        {
            _webSectionRepository = webSectionRepository;
            _dbContext = dbContext;
        }

        [HttpGet]
        public ActionResult<IranLoadingDto> Get(int orderId)
        {
            var order = _dbContext.Orders.Where(e => e.Id == orderId)
                .Include(e=>e.Localization)
                .Include(e => e.Loading)
                .ThenInclude(e => e.DistributeLoads)
                .Include(e => e.Loading)
                .ThenInclude(e => e.AxialForceLoads)
                .Include(e => e.Loading)
                .ThenInclude(e => e.EndMomentLoads)
                .Include(e => e.Loading)
                .ThenInclude(e => e.XPointLoads)
                .FirstOrDefault();

            if (order is null)
            {
                return NotFound();
            }

            var section = _webSectionRepository.Get(order.Localization.SteelType, order.SectionId);

            var loading = order.Loading;

            IranLoadingDto loadingDto;

            if (loading is null)
            {
                loadingDto = new IranLoadingDto
                {
                    Span = order.Span,
                    SelfWeight = section.SelfWeight,
                    DesignType = order.Localization.DesignType,
                };
            }
            else
            {

                loadingDto = new IranLoadingDto
                {
                    Span = order.Span,
                    SelfWeight = section.SelfWeight,
                    CombinationType = loading.CombinationType,
                    DesignType = order.Localization.DesignType,
                };

                var distributeItems = loading.DistributeLoads.Select(e => new IranLoadingItemDto
                {
                    Value = e.Value,
                    XType = e.Type,
                    ZType = ZType.Distribute,
                    Unit = Unit.KN
                });

                if(distributeItems.Any())
                {
                    loadingDto.LoadingItems.AddRange(distributeItems);
                }

                var endMomentItems = loading.EndMomentLoads.Select(e => new IranLoadingItemDto
                {
                    LeftValue = e.LeftValue,
                    RightValue = e.RightValue,
                    XType = e.Type,
                    ZType = ZType.EndMoment,
                    Unit = Unit.KN
                });

                if (endMomentItems.Any())
                {
                    loadingDto.LoadingItems.AddRange(endMomentItems);
                }

                var axialForceItems = loading.AxialForceLoads.Select(e => new IranLoadingItemDto
                {
                    Value = e.Value,
                    XType = e.Type,
                    ZType = ZType.AxialForce,
                    Unit = Unit.KN
                });

                if (axialForceItems.Any())
                {
                    loadingDto.LoadingItems.AddRange(axialForceItems);
                }

                var pointItems = loading.XPointLoads.Select(e => new IranLoadingItemDto
                {
                    Point = e.Position,
                    Value = e.Value,
                    XType = e.Type,
                    ZType = ZType.PointLoad,
                    Unit = Unit.KN
                });

                if (pointItems.Any())
                {
                    loadingDto.LoadingItems.AddRange(pointItems);
                }
            }

            loadingDto.Links.Add(new Link("save-loading", Url.Action(nameof(Loading),
                null, new { orderId },
                Request.Scheme),
                HttpMethods.Post));

            return loadingDto;
        }

        [HttpPost]
        public IActionResult Loading(int orderId, IranLoadingDto loadingDto)
        {
            var order = _dbContext.Orders.Where(e => e.Id == orderId)
                .Include(e => e.Loading)
                .ThenInclude(e => e.DistributeLoads)
                .Include(e => e.Loading)
                .ThenInclude(e => e.AxialForceLoads)
                .Include(e => e.Loading)
                .ThenInclude(e => e.EndMomentLoads)
                .Include(e => e.Loading)
                .ThenInclude(e => e.XPointLoads)
                .FirstOrDefault();

            if (order is null)
            {
                return NotFound();
            }

            if (order.Loading is null)
            {
                order.Loading = new Loading();
            }

            order.Loading.LoadType = LoadType.Iran;
            order.Loading.CombinationType = loadingDto.CombinationType;

            order.Loading.DistributeLoads?.Clear();
            order.Loading.EndMomentLoads?.Clear();
            order.Loading.AxialForceLoads?.Clear();
            order.Loading.XPointLoads?.Clear();

            order.Loading.DistributeLoads = loadingDto.LoadingItems
                .Where(e => e.ZType == ZType.Distribute)
                .Select(e => new DistributeLoad
                {
                    Type = e.XType,
                    Value = e.Value,
                }).ToList();

            order.Loading.EndMomentLoads = loadingDto.LoadingItems
                .Where(e => e.ZType == ZType.EndMoment)
                .Select(e => new EndMomentLoad
                {
                    Type = e.XType,
                    LeftValue = e.LeftValue,
                    RightValue = e.RightValue,
                }).ToList();

            order.Loading.AxialForceLoads = loadingDto.LoadingItems
                .Where(e => e.ZType == ZType.AxialForce)
                .Select(e => new AxialForceLoad
                {
                    Type = e.XType,
                    Value = e.Value,
                }).ToList();

            order.Loading.XPointLoads = loadingDto.LoadingItems
                .Where(e => e.ZType == ZType.PointLoad)
                .Select(e => new XPointLoad
                {
                    Position = e.Point,
                    Type = e.XType,
                    Value = e.Value,
                }).ToList();

            _dbContext.SaveChanges();

            return Ok();
        }
    }

    public class IranLoadingItemDto
    {
        public XType XType { get; set; }
        public ZType ZType { get; set; }
        public double Point { get; set; }
        public int Value { get; set; }
        public int LeftValue { get; set; }
        public int RightValue { get; set; }
        public Unit Unit { get; set; }
    }

    public enum ZType
    {
        Distribute = 1,
        EndMoment = 2,
        AxialForce = 3,
        PointLoad = 4,
    }

    public enum Unit
    {
        KN = 1,
        KG = 2,
        LB = 3
    }

    public class IranLoadingDto : Resource
    {
        public double SelfWeight { get; set; }
        public LoadType LoadType { get; set; }
        public double Span { get; set; }
        public CombinationType CombinationType { get; set; }
        public List<IranLoadingItemDto> LoadingItems { get; set; } = new List<IranLoadingItemDto>();
        public DesignType DesignType { get; internal set; }
    }
}