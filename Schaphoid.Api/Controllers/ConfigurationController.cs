using Microsoft.AspNetCore.Mvc;
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
            var resource = new Resource();

            resource.Links.Add(new Link("beam", Url.Action(nameof(Beam),
                null, null,
                Request.Scheme),
                HttpMethods.Get));

            resource.Links.Add(new Link("localization", Url.Action(nameof(Localization),
                null, null,
                Request.Scheme),
                HttpMethods.Get));

            resource.Links.Add(new Link("order", Url.Action(nameof(Order),
                null, null,
                Request.Scheme),
                HttpMethods.Get));

            return resource;
        }

        #region Order

        [HttpGet("[action]")]
        public OrderDto Order()
        {
            var order = new OrderDto();

            order.Links.Add(new Link("save", Url.Action(nameof(Order),
                null, null,
                Request.Scheme),
                HttpMethods.Post));

            return order;
        }

        [HttpPost("[action]")]
        public object Order(OrderDto orderDto)
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

            return Ok();
        }

        [HttpGet("[action]")]
        public object Orders()
        {
            var orders = _dbContext.Orders.ToList();

            return orders;
        }

        #endregion

        #region Localization

        [HttpGet("[action]")]
        public object Localization()
        {
            var localization = new LocalizationDto()
            {
                DesignType = DesignType.UK,
                DeflectionLimit = new DeflectionLimit(),
                DesignParameters = new DesignParameters(),
            };

            localization.Links.Add(new Link("save", Url.Action(nameof(Beam),
                null, null,
                Request.Scheme),
                HttpMethods.Post));

            return localization;
        }

        [HttpPost("[action]")]
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