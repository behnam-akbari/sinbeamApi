using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            orderDto = GetOrderDto(order.Id);

            return CreatedAtAction(nameof(GetOrder), new
            {
                id = order.Id
            }, orderDto);
        }
        
        [HttpGet("order/{id}")]
        public OrderDto GetOrder(int id)
        {
            var orderDto = GetOrderDto(id);

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

        private OrderDto GetOrderDto(int id)
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

            localization.Links.Add(new Link("save", Url.Action(nameof(Localization),
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
                DeflectionLimit = localizationDto.DeflectionLimit,
                ULSLoadExpression = localizationDto.ULSLoadExpression
            };

            localization.DesignParameters = localizationDto.DesignType switch
            {
                DesignType.UK => Constants.UkNA,
                DesignType.Irish => Constants.IrishNA,
                DesignType.UserDefined => localizationDto.DesignParameters,
            };

            if(localization.ULSLoadExpression == ULSLoadExpression.Expression610a)
            {
                localization.DesignParameters.ReductionFactorF = 1;
            }

            _dbContext.Add(localization);

            _dbContext.SaveChanges();

            return Ok();
        }

        #endregion

        #region Beam

        [HttpGet("order/{id}/[action]")]
        public object Beam(int id)
        {
            var beam = new BeamDto();

            beam.Links.Add(new Link("save", Url.Action(nameof(Beam),
                null, new { id = id },
                Request.Scheme),
                HttpMethods.Post));

            beam.Links.Add(new Link("drawing", Url.Action(nameof(BeamDrawing),
                null, new { id = id },
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

        [HttpPost("order/{id}/[action]")]
        public IActionResult Beam(int id, BeamDto beamDto)
        {
            return Ok();
        }

        [HttpPost("order/{id}/beam/drawing")]
        public object BeamDrawing(int id, BeamDto beamDto)
        {
            if (beamDto.IsUniformDepth)
            {
                beamDto.WebDepthRight = beamDto.WebDepthLeft;
            }

            var maxDepth = Math.Max(beamDto.WebDepthLeft, beamDto.WebDepthRight);

            var maxWidth = Math.Max(beamDto.TopFlangeWidth, beamDto.BottomFlangeWidth);

            double boxh = 500;

            double boxw = boxh * 180 / 225;

            var scalerh = (0.8 * boxh) / maxDepth;
            var scalerw = (0.8 * boxw) / maxWidth;

            var scaler = Math.Min(scalerh, scalerw);

            var top_w = Math.Round(scaler * beamDto.TopFlangeWidth, 2);
            var bottom_w = Math.Round(scaler * beamDto.BottomFlangeWidth, 2);
            var top_flg = Math.Round(scaler * beamDto.TopFlangeThickness, 2);
            var bottom_flg = Math.Round(scaler * beamDto.BottomFlangeThickness, 2);
            var webs = Math.Round(scaler * beamDto.WebThickness, 2);
            var depths = Math.Round(scaler * maxDepth, 2);

            var top_of_beam = Math.Round(0.5 * boxh - 0.5 * depths, 2);
            var in_top_flg = Math.Round(top_of_beam + top_flg, 2);
            var bottom_of_beam = Math.Round(top_of_beam + depths, 2);
            var in_bottom_flg = Math.Round(bottom_of_beam - bottom_flg, 2);

            var left_top = Math.Round(0.5 * boxw - 0.5 * top_w, 2);
            var right_top = Math.Round(left_top + top_w, 2);
            var left_bottom = Math.Round(0.5 * boxw - 0.5 * bottom_w, 2);
            var right_bottom = Math.Round(left_bottom + bottom_w, 2);

            var left_web = Math.Round(0.5 * boxw - 0.5 * webs, 2);
            var right_web = Math.Round(left_web + webs, 2);

            return new
            {
                topFlange = new
                {
                    width = Math.Round(right_top - left_top, 2),
                    height = Math.Round(Math.Max(in_top_flg - top_of_beam - 3, 0), 2),
                    borderWidth = "2px 2px 1px 2px"
                },
                web = new
                {
                    width = 0,
                    height = Math.Round(in_bottom_flg - in_top_flg, 2),
                    borderWidth = "auto 2px auto 2px"
                },
                bottomFlange = new
                {
                    width = Math.Round(right_top - left_top, 2),
                    height = Math.Round(Math.Max(bottom_of_beam - in_bottom_flg - 3, 0), 2),
                    borderWidth = "1px 2px 2px 2px"
                }
            };
        }

        #endregion
    }

    public class BeamDto : Resource
    {
        public double Span { get; set; }
        public bool IsUniformDepth { get; set; } = true;
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
        public ULSLoadExpression ULSLoadExpression { get; set; } = ULSLoadExpression.Expression610a;
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