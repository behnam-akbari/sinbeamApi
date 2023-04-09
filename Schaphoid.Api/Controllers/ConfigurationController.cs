using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Scaphoid.Core.Model;
using Scaphoid.Infrastructure.Data;
using System.Text.Json.Serialization;

namespace Schaphoid.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConfigurationController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public ConfigurationController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public Resource Get()
        {
            var order = new OrderDto()
            {
                OrderDate = DateTime.Today
            };

            order.Links.Add(new Link("create-order", Url.Action(nameof(CreateOrder),
                null, null,
                Request.Scheme),
                HttpMethods.Post));

            return order;
        }

        #region Order

        [HttpPost("order")]
        public IActionResult CreateOrder(OrderDto orderDto)
        {
            var order = new Order()
            {
                Beam = orderDto.Beam,
                Company = orderDto.Company,
                Designer = orderDto.Designer,
                Note = orderDto.Note,
                OrderDate = orderDto.OrderDate,
                Project = orderDto.Project
            };

            _dbContext.Add(order);
            _dbContext.SaveChanges();

            return CreatedAtAction(nameof(GetOrder), new
            {
                id = order.Id
            }, null);
        }
        
        [HttpGet("order/{id}")]
        public OrderDto GetOrder(int id)
        {
            var order = _dbContext.Orders.Find(id);

            var orderDto = new OrderDto
            {
                Beam = order.Beam,
                Company = order.Company,
                Designer = order.Designer,
                Note = order.Note,
                OrderDate = order.OrderDate,
                Project = order.Project,
            };

            orderDto.Links.Add(new Link("save-order", Url.Action(nameof(SaveOrder),
                null, new { id = id },
                Request.Scheme),
                HttpMethods.Post));

            orderDto.Links.Add(new Link("get-localization", Url.Action(nameof(Localization),
                null, new { id = id },
                Request.Scheme),
                HttpMethods.Get));

            orderDto.Links.Add(new Link("get-beam", Url.Action(nameof(Beam),
                null, new { id = id },
                Request.Scheme),
                HttpMethods.Get));

            return orderDto;
        }

        [HttpPost("order/{id}")]
        public IActionResult SaveOrder(int id, OrderDto orderDto)
        {
            var order = _dbContext.Orders.Find(id);

            order.Note = orderDto.Note;
            order.Designer = orderDto.Designer;
            order.Project = orderDto.Project;
            order.OrderDate = orderDto.OrderDate;
            order.Beam = orderDto.Beam;
            order.Company = orderDto.Company;

            _dbContext.SaveChanges();

            return AcceptedAtAction(nameof(GetOrder), null, new
            {
                id = order.Id
            });
        }

        [HttpGet("[action]")]
        public object Orders()
        {
            var orders = _dbContext.Orders.ToList();

            return orders;
        }

        #endregion

        #region Localization

        [HttpGet("order/{id}/[action]")]
        public object Localization(int id)
        {
            var order = _dbContext.Orders.Where(e => e.Id == id)
                .Include(e => e.Localization)
                .FirstOrDefault();

            if(order is null)
            {
                return NotFound();
            }

            var localization = new LocalizationDto();

            if (order.Localization is not null)
            {
                localization.DesignType = order.Localization.DesignType;
                localization.DeflectionLimit = order.Localization.DeflectionLimit;
                localization.DesignParameters = order.Localization.DesignParameters;
                localization.DefaultNA = Constants.DefaultNA;
            }
            else
            {
                localization.DesignType = DesignType.UK;
                localization.DeflectionLimit = new DeflectionLimit();
                localization.DesignParameters = new DesignParameters();
                localization.DefaultNA = Constants.DefaultNA;
            }

            localization.Links.Add(new Link("save", Url.Action(nameof(Beam),
                null, new { id = id },
                Request.Scheme),
                HttpMethods.Post));

            return localization;
        }

        [HttpPost("order/{id}/[action]")]
        public IActionResult Localization(LocalizationDto localizationDto)
        {
            var localization = new Localization()
            {
                OrderId = 1,
                DesignType = localizationDto.DesignType,
                DeflectionLimit = localizationDto.DeflectionLimit
            };

            if(localizationDto.DesignType == DesignType.UserDefined)
            {
                localization.DesignParameters = new DesignParameters()
                {
                    GammaG = localizationDto.DesignParameters.GammaG,
                    GammaQ = localizationDto.DesignParameters.GammaQ,
                    ModificationFactorAllOtherHtoB = localizationDto.DesignParameters.ModificationFactorAllOtherHtoB,
                    ModificationFactorKflHtoBLessThanTwo = localizationDto.DesignParameters.ModificationFactorKflHtoBLessThanTwo,
                    ReductionFactorF = localizationDto.DesignParameters.ReductionFactorF,
                    SteelGradeS235Between16and40mm = localizationDto.DesignParameters.SteelGradeS235Between16and40mm,
                    SteelGradeS235Between40and63mm = localizationDto.DesignParameters.SteelGradeS235Between40and63mm,
                    SteelGradeS235LessThan16mm = localizationDto.DesignParameters.SteelGradeS235LessThan16mm,
                    SteelGradeS355Between16and40mm = localizationDto.DesignParameters.SteelGradeS355Between16and40mm,
                    SteelGradeS355Between40and63mm = localizationDto.DesignParameters.SteelGradeS355Between40and63mm,
                    SteelGradeS355LessThan16mm = localizationDto.DesignParameters.SteelGradeS355LessThan16mm
                };
            }

            _dbContext.Add(localization);

            _dbContext.SaveChanges();

            return Ok();
        }

        #endregion

        #region Beam

        [HttpGet("[action]")]
        public object Beam()
        {
            var beam = new BeamDto();

            beam.Links.Add(new Link("save", Url.Action(nameof(Beam),
                null, null,
                Request.Scheme),
                HttpMethods.Post));

            return new
            {
                Beam = beam,
                Constants.WebThicknessCollection,
                Constants.FlangeThicknessCollection,
                Constants.FlangeWidthCollection
            };
        }

        [HttpPost("[action]")]
        public IActionResult Beam(BeamDto beamDto)
        {
            return Ok();
        }

        #endregion
    }

    public class BeamDto : Resource
    {
        public double Span { get; set; }
        public bool IsUniformDepth { get; set; } = true;
        public int WebDepth { get; set; } = 1000;
        public int WebDepthLeft { get; set; } = 1000;
        public int WebDepthRight { get; set; } = 1000;
        public double WebThickness { get; set; } = 2.5;
        public int TopFlangeThickness { get; set; } = 12;
        public int TopFlangeWidth { get; set; } = 200;
        public int BottomFlangeThickness { get; set; } = 12;
        public int BottomFlangeWidth { get; set; } = 200;
    }

    public class LocalizationDto : Resource
    {
        public DesignType DesignType { get; set; }
        public DesignParameters DesignParameters { get; set; }
        public DeflectionLimit DeflectionLimit { get; set; }
        public DesignParameters DefaultNA { get; set; }
    }

    public class ConfigurationDto : Resource
    {
    }

    public class Link
    {
        public Link(string rel, string href, string method)
        {
            Rel = rel;
            Href = href;
            Method = method;
        }
        public string Href { get; set; }
        public string Rel { get; set; }
        public string Method { get; set; }
    }

    public class Resource
    {
        [JsonPropertyName("_links")]
        public List<Link> Links { get; set; } = new List<Link>();
    }

    public class OrderDto : Resource
    {
        public string Company { get; set; }
        public string Project { get; set; }
        public string Beam { get; set; }
        public string Designer { get; set; }
        public string Note { get; set; }
        public DateTime OrderDate { get; set; }
    }
}