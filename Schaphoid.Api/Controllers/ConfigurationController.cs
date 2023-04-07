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

            return resource;
        }

        [HttpGet("[action]")]
        public object Orders()
        {
            var orders = _dbContext.Order.ToList();

            return orders;
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
            return Ok();
        }

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

    public class OrderDto
    {
        public string Company { get; set; }
        public string Project { get; set; }
        public string Beam { get; set; }
        public string Designer { get; set; }
        public string Note { get; set; }
        public DateTime OrderDate { get; set; }
    }
}