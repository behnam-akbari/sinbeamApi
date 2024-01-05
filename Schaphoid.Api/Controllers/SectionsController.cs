using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scaphoid.Core.Model;
using Scaphoid.Infrastructure.Data;
using Scaphoid.Infrastructure.Repositories;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using static Schaphoid.Api.Controllers.SectionDesign;

namespace Schaphoid.Api.Controllers
{
    [ApiController]
    [Route("order/{orderId}/[controller]")]
    public class SectionsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly WebSectionRepository _webSectionRepository;

        public SectionsController(WebSectionRepository webSectionRepository, ApplicationDbContext dbContext)
        {
            _webSectionRepository = webSectionRepository;
            _dbContext = dbContext;
        }

        [HttpGet("[action]")]
        public ActionResult<List<WebSection>> Query(int orderId, [FromQuery]WebSectionFilters filters)
        {
            var order = _dbContext.Orders
                .Where(e => e.Id == orderId)
                .Include(e=>e.Localization)
                .FirstOrDefault();

            if (order is null)
            {
                return NotFound();
            }

            var sections = _webSectionRepository.Get(order.Localization.SteelType);

            if(filters.Weight is not null)
            {
                sections = sections.Where(e => e.Weight >= filters.Weight.Min)
                    .Where(e => e.Weight <= filters.Weight.Max);
            }

            if (filters.SectionPerimeter is not null)
            {
                sections = sections.Where(e => e.SectionPerimeter >= filters.SectionPerimeter.Min)
                    .Where(e => e.SectionPerimeter <= filters.SectionPerimeter.Max);
            }

            if (filters.ShearCapacity is not null)
            {
                sections = sections.Where(e => e.ShearCapacity >= filters.ShearCapacity.Min)
                    .Where(e => e.ShearCapacity <= filters.ShearCapacity.Max);
            }

            if (filters.MomentOfInertiaIy is not null)
            {
                sections = sections.Where(e => e.MomentOfInertiaIy >= filters.MomentOfInertiaIy.Min)
                    .Where(e => e.MomentOfInertiaIy <= filters.MomentOfInertiaIy.Max);
            }

            if (filters.MomentOfInertiaIz is not null)
            {
                sections = sections.Where(e => e.MomentOfInertiaIz >= filters.MomentOfInertiaIz.Min)
                    .Where(e => e.MomentOfInertiaIz <= filters.MomentOfInertiaIz.Max);
            }

            if (filters.AxialCapacity is not null)
            {
                sections = sections.Where(e => e.AxialCapacity >= filters.AxialCapacity.Min)
                    .Where(e => e.AxialCapacity <= filters.AxialCapacity.Max);
            }

            if (filters.WebThickness is not null)
            {
                sections = sections.Where(e => e.WebThickness >= filters.WebThickness.Min)
                    .Where(e => e.WebThickness <= filters.WebThickness.Max);
            }

            if (filters.WebHeight is not null)
            {
                sections = sections.Where(e => e.WebHeight >= filters.WebHeight.Min)
                    .Where(e => e.WebHeight <= filters.WebHeight.Max);
            }

            if (filters.BendingCapacity is not null)
            {
                sections = sections.Where(e => e.BendingCapacity >= filters.BendingCapacity.Min)
                    .Where(e => e.BendingCapacity <= filters.BendingCapacity.Max);
            }

            if (filters.FlangeWidth is not null)
            {
                sections = sections.Where(e => e.FlangeWidth >= filters.FlangeWidth.Min)
                    .Where(e => e.FlangeWidth <= filters.FlangeWidth.Max);
            }

            if (filters.FlangeThickness is not null)
            {
                sections = sections.Where(e => e.FlangeThickness >= filters.FlangeThickness.Min)
                    .Where(e => e.FlangeThickness <= filters.FlangeThickness.Max);
            }

            var output = sections.ToList();

            foreach (var section in output)
            {
                section.Links.Add(new Link("get-section", Url.Action(nameof(Get),
                    null, new { orderId = orderId, sectionId = section.Id },
                    Request.Scheme),
                    HttpMethods.Get));
            }

            return output;
        }

        [HttpGet("[action]")]
        public ActionResult<WebSectionFilters> Filters(int orderId)
        {
            var order = _dbContext.Orders
                .Where(e => e.Id == orderId)
                .Include(e=>e.Localization)
                .FirstOrDefault();

            if (order is null)
            {
                return NotFound();
            }

            var sections = _webSectionRepository.Get(order.Localization.SteelType);

            var filters = new WebSectionFilters
            {
                Weight = new WebSectionFilter(sections.Min(e => e.Weight), sections.Max(e => e.Weight)),
                WebHeight = new WebSectionFilter(sections.Min(e => e.WebHeight), sections.Max(e => e.WebHeight)),
                WebThickness = new WebSectionFilter()
                {
                    Min = 1.5,
                    Max = 6
                },
                FlangeThickness = new WebSectionFilter(sections.Min(e => e.FlangeThickness), sections.Max(e => e.FlangeThickness)),
                FlangeWidth = new WebSectionFilter(sections.Min(e => e.FlangeWidth), sections.Max(e => e.FlangeWidth)),
                MomentOfInertiaIy = new WebSectionFilter(sections.Min(e => e.MomentOfInertiaIy), sections.Max(e => e.MomentOfInertiaIy)),
                MomentOfInertiaIz = new WebSectionFilter(sections.Min(e => e.MomentOfInertiaIz), sections.Max(e => e.MomentOfInertiaIz)),
                SectionPerimeter = new WebSectionFilter(sections.Min(e => e.SectionPerimeter), sections.Max(e => e.SectionPerimeter)),
                ShearCapacity = new WebSectionFilter(sections.Min(e => e.ShearCapacity), sections.Max(e => e.ShearCapacity)),
                BendingCapacity = new WebSectionFilter(sections.Min(e => e.BendingCapacity), sections.Max(e => e.BendingCapacity)),
                AxialCapacity = new WebSectionFilter(sections.Min(e => e.AxialCapacity), sections.Max(e => e.AxialCapacity)),
            };

            return filters;
        }

        [HttpGet("{sectionId}")]
        public ActionResult<WebSectionDto> Get(int orderId, string sectionId)
        {
            var order = _dbContext.Orders
                .Where(e => e.Id == orderId)
                .Include(e => e.Localization)
                .FirstOrDefault();

            if (order is null)
            {
                return NotFound();
            }

            var section = GetSectionDto(order.Localization.SteelType, sectionId);

            section.Links.Add(new Link("get-sections-filters", Url.Action(nameof(Filters),
                null, new { orderId },
                Request.Scheme),
                HttpMethods.Get));

            section.Links.Add(new Link("query-sections", Url.Action(nameof(Query),
                    null, new { orderId },
                    Request.Scheme),
                    HttpMethods.Get));

            section.Links.Add(new Link("get-section", Url.Action(nameof(Get),
                null, new { orderId = orderId, sectionId = section.Id },
                Request.Scheme),
                HttpMethods.Get));

            section.Links.Add(new Link("save-section", Url.Action(nameof(Save),
                null, new { orderId = orderId, sectionId = section.Id },
                Request.Scheme),
                HttpMethods.Post));

            return section;
        }

        [HttpGet("[action]")]
        public ActionResult<Resource> Init(int orderId)
        {
            var order = _dbContext.Orders
                .Where(e => e.Id == orderId)
                .FirstOrDefault();

            if (order is null)
            {
                return NotFound();
            }

            var resource = new Resource();

            resource.Links.Add(new Link("get-sections-filters", Url.Action(nameof(Filters),
                    null, new { orderId },
                    Request.Scheme),
                    HttpMethods.Get));

            resource.Links.Add(new Link("query-sections", Url.Action(nameof(Query),
                    null, new { orderId },
                    Request.Scheme),
                    HttpMethods.Get));

            return resource;
        }

        [HttpPost("{sectionId}")]
        public IActionResult Save(int orderId, string sectionId)
        {
            var order = _dbContext.Orders
                .Where(e => e.Id == orderId)
                .FirstOrDefault();

            if (order is null)
            {
                return NotFound();
            }

            order.SectionId = sectionId;

            _dbContext.SaveChanges();

            return Ok();
        }

        #region Private Methods

        private SectionDesign GetSectionDesign(WebSection section)
        {
            var height = 500;
            double scaler = 1;

            if (height > section.WebHeight)
            {
                scaler = Math.Round(height / section.WebHeight, 2);
            }

            return new SectionDesign
            {
                TopFlange = new Part
                {
                    Width = section.FlangeWidth,
                    Height = section.FlangeThickness,
                    ScaledHeight = Math.Round(scaler * section.FlangeThickness, 0),
                    ScaledWidth = Math.Round(scaler * section.FlangeWidth, 0),
                },
                Web = new Part
                {
                    Width = section.WebThickness,
                    Height = section.WebHeight,
                    ScaledHeight = Math.Round(scaler * section.WebHeight, 0),
                    ScaledWidth = Math.Round(scaler * section.WebThickness, 0),
                },
                BottomFlange = new Part
                {
                    Width = section.FlangeWidth,
                    Height = section.FlangeThickness,
                    ScaledHeight = Math.Round(scaler * section.FlangeThickness, 0),
                    ScaledWidth = Math.Round(scaler * section.FlangeWidth, 0),
                }
            };
        }

        private IEnumerable<string> GetSectionProperties(WebSection section)
        {
            var properties = new List<string>()
            {
                "Beam has uniform depth",
                $"Iy Major axis inertia = {Math.Round(section.MomentOfInertiaIy, 3)} cm4",
                $"Iz Minor axis inertia = {Math.Round(section.MomentOfInertiaIz, 3)} cm4",
                
                $"iy Major axis gyration = {Math.Round(Math.Sqrt(section.MomentOfInertiaIy/section.SectionPerimeter), 3)} cm",
                $"iz Minor axis gyration = {Math.Round(Math.Sqrt(section.MomentOfInertiaIz/section.SectionPerimeter), 3)} cm",
                $"It Torsional inertia = {Math.Round(section.It, 3)} cm4",
                
                $"Iw Warping inertia = {Math.Round(section.Iw, 3)} cm6",
                $"A Cross section area = {Math.Round(section.SectionPerimeter, 3)} cm2",
                $"Weight per m = {Math.Round(section.Weight, 3)} kg/m",
                
                $"Surface area per m = {Math.Round(section.SurfaceAreaPerM, 3)} m2/m",
                $"Surface area per T = {Math.Round(section.SurfaceAreaPerT, 3)} m2/T"
            };

            return properties;
        }

        private WebSectionDto GetSectionDto(SteelType steelType, string id)
        {
            var section = _webSectionRepository.Get(steelType, id);

            var sectionDesign = GetSectionDesign(section);

            var properties = GetSectionProperties(section);

            var steel = steelType.GetDisplayName();

            var output = new WebSectionDto
            {
                Id = id,
                Key = section.Key,
                WebDepth = section.WebHeight,
                WebThickness = section.WebThickness,
                WebSteel = steel,

                TopFlangeWidth = section.FlangeWidth,
                TopFlangeThickness = section.FlangeThickness,
                TopFlangeSteel = steel,

                BottomFlangeWidth = section.FlangeWidth,
                BottomFlangeThickness = section.FlangeThickness,
                BottomFlangeSteel = steel,

                Properties = properties,
                Design = sectionDesign,
            };

            return output;
        }

        #endregion
    }

    public class SectionDesign
    {
        public Part TopFlange { get; set; }
        public Part Web { get; set; }
        public Part BottomFlange { get; set; }

        public class Part
        {
            public double Width { get; set; }
            public double Height { get; set; }
            public double ScaledWidth { get; set; }
            public double ScaledHeight { get; set; }
        }
    }

    public class WebSectionFilter
    {
        public WebSectionFilter()
        {
        }

        public WebSectionFilter(double min, double max)
        {
            Min = Math.Floor(min);
            Max = Math.Ceiling(max);
        }

        public double Min { get; set; }
        public double Max { get; set; }
    }

    public class WebSectionFilters
    {
        public WebSectionFilter Weight { get; set; }
        public WebSectionFilter WebHeight { get; set; }
        public WebSectionFilter WebThickness  { get; set; }
        public WebSectionFilter FlangeWidth { get; set; }
        public WebSectionFilter FlangeThickness { get; set; }
        public WebSectionFilter SectionPerimeter { get; set; }
        public WebSectionFilter MomentOfInertiaIy { get; set; }
        public WebSectionFilter MomentOfInertiaIz { get; set; }
        public WebSectionFilter BendingCapacity { get; set; }
        public WebSectionFilter ShearCapacity { get; set; }
        public WebSectionFilter AxialCapacity { get; set; }
    }

    public class WebSectionDto : Resource
    {
        public string Id { get; set; }
        public string Key { get; set; }
        
        public double WebDepth { get; set; }
        public double WebThickness { get; set; }
        public string WebSteel { get; set; }

        public double TopFlangeThickness { get; set; }
        public double TopFlangeWidth { get; set; }
        public string TopFlangeSteel { get; set; }

        public double BottomFlangeThickness { get; set; }
        public double BottomFlangeWidth { get; set; }
        public string BottomFlangeSteel { get; set; }

        public SectionDesign Design { get; set; }

        public IEnumerable<string> Properties { get; set; }
    }

    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .First()
                            .GetCustomAttribute<DisplayAttribute>()
                            .GetName();
        }
    }

}
