using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Scaphoid.Core.Model;
using Scaphoid.Infrastructure.Data;
using Scaphoid.Infrastructure.Repositories;

namespace Schaphoid.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConfigurationController : BaseController
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly WebSectionRepository _webSectionRepository;

        public ConfigurationController(ApplicationDbContext dbContext, WebSectionRepository webSectionRepository)
        {
            _dbContext = dbContext;
            _webSectionRepository = webSectionRepository;
        }

        [HttpGet]
        public OrderDto Get(int? id)
        {
            var order = new OrderDto();

            if (id.HasValue)
            {
                order = GetOrderDto(id.Value);
            }
            else
            {
                order = new OrderDto();

                order.Links.Add(new Link("create-order", Url.Action(nameof(CreateOrder),
                    null, null,
                    Request.Scheme),
                    HttpMethods.Post));
            }

            order.Links.Add(new Link("ask-question", Url.Action(nameof(QuestionController.Create),
            "question", null,
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
                ElementType = orderDto.ElementType,
                Span = orderDto.Span,
                Designer = orderDto.Designer,
                Note = orderDto.Note,
                CreatedOn = DateTime.Now,
                Project = orderDto.ProjectName
            };

            order.Localization = new Localization()
            {
                DesignType = orderDto.DesignType,
                DeflectionLimit = orderDto.DeflectionLimit,
                ULSLoadExpression = orderDto.ULSLoadExpression,
                SteelType = orderDto.SteelType,
                DesignParameters = orderDto.DesignType switch
                {
                    DesignType.UK => Constants.UkNA,
                    DesignType.Irish => Constants.IrishNA,
                    DesignType.Iran => Constants.IranNA,
                    DesignType.UserDefined => orderDto.DesignParameters,
                    _ => null,
                }
            };

            _dbContext.Add(order);

            try
            {
                _dbContext.SaveChanges();
            }
            catch (Exception ex) 
            {
                throw;
            }

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
            var order = _dbContext.Orders
                .Include(e => e.Localization)
                .FirstOrDefault(e => e.Id == id);

            order.Note = orderDto.Note;
            order.Designer = orderDto.Designer;
            order.Project = orderDto.ProjectName;

            order.Localization = new Localization()
            {
                DesignType = orderDto.DesignType,
                DeflectionLimit = orderDto.DeflectionLimit,
                ULSLoadExpression = orderDto.ULSLoadExpression,
                SteelType = orderDto.SteelType,
                DesignParameters = orderDto.DesignType switch
                {
                    DesignType.UK => Constants.UkNA,
                    DesignType.Irish => Constants.IrishNA,
                    DesignType.Iran => Constants.IranNA,
                    DesignType.UserDefined => orderDto.DesignParameters,
                    _ => null,
                },
            };

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
            var order = _dbContext.Orders
                .Include(e => e.Localization)
                .FirstOrDefault(e => e.Id == id);

            var orderDto = new OrderDto
            {
                Id = id,
                Designer = order.Designer,
                Note = order.Note,
                ProjectName = order.Project,
                ElementType = ElementType.Rafter,
                Span = order.Span,
                DesignType = order.Localization.DesignType,
                DeflectionLimit = order.Localization.DeflectionLimit,
                DesignParameters = order.Localization.DesignParameters,
                ULSLoadExpression = order.Localization.ULSLoadExpression,
                SteelType = order.Localization.SteelType,
                Step = Step.GeneralDetails
            };

            orderDto.Links.Add(new Link("get-order", Url.Action(nameof(GetOrder),
                null, new { id = id },
                Request.Scheme),
                HttpMethods.Get));

            orderDto.Links.Add(new Link("save-order", Url.Action(nameof(SaveOrder),
                null, new { id = id },
                Request.Scheme),
                HttpMethods.Post));

            if(order.SectionId == null)
            {
                orderDto.Links.Add(new Link("get-section", Url.Action(nameof(SectionsController.Init),
                    "sections", new { orderId = id },
                    Request.Scheme),
                    HttpMethods.Get));
            }
            else
            {
                orderDto.Links.Add(new Link("get-section", Url.Action(nameof(SectionsController.Get),
                    "sections", new { orderId = id, order.SectionId },
                    Request.Scheme),
                    HttpMethods.Get));
            }

            if(order.Localization.DesignType == DesignType.Iran)
            {
                orderDto.Links.Add(new Link("get-loading", Url.Action(nameof(IranLoadingController.Get),
                    "iranLoading", new { orderId = id },
                    Request.Scheme),
                    HttpMethods.Get));

                orderDto.Links.Add(new Link("get-analysis", Url.Action(nameof(AnalysisController.Iran),
                    "analysis", new { orderId = id, combination = CombinationType.C1 },
                    Request.Scheme),
                    HttpMethods.Get));
            }
            else
            {
                orderDto.Links.Add(new Link("get-loading", Url.Action(nameof(LoadingController.Get),
                    "loading", new { orderId = id },
                    Request.Scheme),
                    HttpMethods.Get));

                orderDto.Links.Add(new Link("get-analysis", Url.Action(nameof(AnalysisController.Standard),
                    "analysis", new { orderId = id },
                    Request.Scheme),
                    HttpMethods.Get));
            }

            orderDto.Links.Add(new Link("get-restraints", Url.Action(nameof(VerificationController.Restraints),
                "Verification", new { orderId = id },
                Request.Scheme),
                HttpMethods.Get));

            orderDto.Links.Add(new Link("get-top-flange-verification", Url.Action(nameof(VerificationController.TopFlangeVerification),
                "Verification", new { orderId = id },
                Request.Scheme),
                HttpMethods.Get));

            orderDto.Links.Add(new Link("get-bottom-flange-verification", Url.Action(nameof(VerificationController.BottomFlangeVerification),
                "Verification", new { orderId = id },
                Request.Scheme),
                HttpMethods.Get));

            orderDto.Links.Add(new Link("get-web-verification", Url.Action(nameof(VerificationController.WebVerification),
                "Verification", new { orderId = id },
                Request.Scheme),
                HttpMethods.Get));

            orderDto.Links.Add(new Link("get-design", Url.Action(nameof(DesignController.Get),
                "Design", new { orderId = id },
                Request.Scheme),
                HttpMethods.Get));

            return orderDto;
        }

        #endregion
    }

    public class HelpData
    {
        public double uls_left_mom { get; set; }
        public double uls_right_mom { get; set; }
        public double lh_reaction { get; set; }
        public double uls_udl { get; set; }
        public int part_udl_start { get; set; }
        public int part_udl_end { get; internal set; }
        public double part_uls_udl { get; internal set; }
        public double rh_reaction { get; internal set; }
        public double part_sls_udl { get; internal set; }
        public double part_unfactored_udl { get; internal set; }
        public double unfactored_uls { get; internal set; }
        public double sls_udl { get; internal set; }
        public double gamma_g { get; internal set; }
        public double gamma_q { get; internal set; }
        public double gamma_g_610a { get; internal set; }
        public double gamma_q_610a { get; internal set; }
    }

    public class BeamDto : Resource
    {
        public double Span { get; set; }
        public bool IsUniformDepth { get; set; } = true;
        public int WebDepth { get; set; } = 1000;
        public double WebThickness { get; set; } = 2.5;
        public int TopFlangeThickness { get; set; } = 12;
        public int TopFlangeWidth { get; set; } = 200;
        public int BottomFlangeThickness { get; set; } = 12;
        public int BottomFlangeWidth { get; set; } = 200;
        public SteelType WebSteel { get; set; } = SteelType.S355;
        public SteelType BottomFlangeSteel { get; set; } = SteelType.S355;
        public SteelType TopFlangeSteel { get; set; } = SteelType.S355;
    }

    public class LocalizationDto
    {
        public DesignType DesignType { get; set; }
        public DesignParameters DesignParameters { get; set; }
        public DeflectionLimit DeflectionLimit { get; set; }
        public DesignParameters DefaultNA { get; set; }
        public ULSLoadExpression ULSLoadExpression { get; set; } = ULSLoadExpression.Expression610a;
        public int PsiValue { get; set; }
    }

    public class ConfigurationDto : Resource
    {
    }

    public class LoadingDto : Resource
    {
        public double SelfWeight { get; set; }
        public LoadType LoadType { get; set; }
        public LoadParameters PermanentLoads { get; set; }
        public LoadParameters VariableLoads { get; set; }
        public LoadParameters UltimateLoads { get; set; }
        public List<UltimatePointLoadDto> UltimatePointLoads { get; set; }
        public List<CharacteristicPointLoadDto> CharacteristicPointLoads { get; set; }
        public double Span { get; set; }
        public CombinationType CombinationType { get; set; }
        public DesignType DesignType { get; internal set; }
    }

    public class UltimatePointLoadDto
    {
        public int Id { get; set; }
        public double Position { get; set; }
        public double Load { get; set; }
    }

    public class CharacteristicPointLoadDto
    {
        public int Id { get; set; }
        public double Position { get; set; }
        public double PermanentAction { get; set; }
        public double VariableAction { get; set; }
    }

    public class OrderDto : Resource
    {
        public int Id { get; set; }
        public string ProjectName { get; set; }
        public string Designer { get; set; }
        public string Note { get; set; }
        public ElementType ElementType { get; set; } = ElementType.Rafter;
        public double Span { get; set; }


        public DesignType DesignType { get; set; } = DesignType.UK;
        public DesignParameters DesignParameters { get; set; }
        public DeflectionLimit DeflectionLimit { get; set; }
        //public DesignParameters DefaultNA { get; set; }
        public ULSLoadExpression ULSLoadExpression { get; set; } = ULSLoadExpression.Expression610a;
        public SteelType SteelType { get; set; }
        public Step Step { get; set; }
    }

    public class Point
    {
        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; }
        public double Y { get; }
    }

    public class VerificationItem
    {
        public double From { get; set; }
        public double To { get; set; }
        public double DesignForce { get; set; }
        public double Resistance { get; set; }
        public double Utilization { get; set; }
        public List<string> Captions { get; set; }
    }

    public class FlangeDesign
    {
        public string Caption { get; internal set; }
        public int Thick { get; internal set; }
        public double Width { get; internal set; }
        public double Utilization { get; internal set; }
        public bool IsValid { get; internal set; }
    }

    public class Resource<T> : Resource
    {
        public T Data { get; set; }
    }

    public enum Step
    {
        GeneralDetails = 1,
        DefineSection = 2,
        Loading = 3,
        Analysis = 4,
        Verification = 5,
        Design = 6
    }
}