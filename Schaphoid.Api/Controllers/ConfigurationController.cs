using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scaphoid.Core.Model;
using Scaphoid.Infrastructure.Data;
using System;
using System.Diagnostics.Metrics;
using System.Text.Json.Serialization;
using static System.Net.Mime.MediaTypeNames;

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

            orderDto.Links.Add(new Link("get-order", Url.Action(nameof(GetOrder),
                null, new { id = id },
                Request.Scheme),
                HttpMethods.Get));

            orderDto.Links.Add(new Link("save-order", Url.Action(nameof(SaveOrder),
                null, new { id = id },
                Request.Scheme),
                HttpMethods.Post));

            orderDto.Links.Add(new Link("get-localization", Url.Action(nameof(Localization),
                null, new { id = id },
                Request.Scheme),
                HttpMethods.Get));

            orderDto.Links.Add(new Link("save-localization", Url.Action(nameof(Localization),
                null, new { id = id },
                Request.Scheme),
                HttpMethods.Post));

            orderDto.Links.Add(new Link("get-beam", Url.Action(nameof(Beam),
                null, new { id = id },
                Request.Scheme),
                HttpMethods.Get));

            orderDto.Links.Add(new Link("save-beam", Url.Action(nameof(Beam),
                null, new { id = id },
                Request.Scheme),
                HttpMethods.Post));

            orderDto.Links.Add(new Link("drawing", Url.Action(nameof(BeamDrawing),
                null, new { id = id },
                Request.Scheme),
                HttpMethods.Post));


            orderDto.Links.Add(new Link("get-loading", Url.Action(nameof(Loading),
                null, new { id = id },
                Request.Scheme),
                HttpMethods.Get));

            orderDto.Links.Add(new Link("save-loading", Url.Action(nameof(Loading),
                null, new { id = id },
                Request.Scheme),
                HttpMethods.Post));

            orderDto.Links.Add(new Link("get-properties", Url.Action(nameof(Properties),
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

            return localization;
        }

        [HttpPost("order/{id}/[action]")]
        public IActionResult Localization(int id, LocalizationDto localizationDto)
        {
            var localization = new Localization()
            {
                OrderId = id,
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
                localization.PsiValue = 1;
            }
            else
            {
                localization.PsiValue = localizationDto.PsiValue;
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
            var order = _dbContext.Orders.Where(e => e.Id == id)
                .Include(e => e.BeamInfo)
                .FirstOrDefault();

            if(order is null)
            {
                return NotFound();
            }

            var beam = new BeamDto()
            {
                Span = 20
            };

            if (order.BeamInfo is not null)
            {
                beam = new BeamDto()
                {
                    Span = order.BeamInfo.Span,
                    WebDepthLeft = order.BeamInfo.WebDepth,
                    WebDepthRight = order.BeamInfo.WebDepthRight,
                    BottomFlangeThickness = order.BeamInfo.BottomFlangeThickness,
                    BottomFlangeWidth = order.BeamInfo.BottomFlangeWidth,
                    IsUniformDepth = order.BeamInfo.IsUniformDepth,
                    TopFlangeThickness = order.BeamInfo.TopFlangeThickness,
                    TopFlangeWidth = order.BeamInfo.TopFlangeWidth,
                    WebThickness = order.BeamInfo.WebThickness
                };
            }

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
            var order = _dbContext.Orders.Where(e => e.Id == id)
                .Include(e=>e.Localization)
                .FirstOrDefault();

            if(order is null)
            {
                return NotFound();
            }

            order.BeamInfo = new Beam
            {
                OrderId = order.Id,
                IsUniformDepth = beamDto.IsUniformDepth,
                BottomFlangeThickness = beamDto.BottomFlangeThickness,
                BottomFlangeWidth = beamDto.BottomFlangeWidth,
                Span = beamDto.Span,
                TopFlangeThickness = beamDto.TopFlangeThickness,
                TopFlangeWidth = beamDto.TopFlangeWidth,
                WebDepth = beamDto.WebDepthLeft,
                WebDepthRight = beamDto.IsUniformDepth? beamDto.WebDepthLeft : beamDto.WebDepthRight,
                WebThickness = beamDto.WebThickness
            };

            _dbContext.SaveChanges();

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

            var thick = beamDto.WebThickness * 2;

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
                    borderWidth = $"0 {thick}px 0 0"
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

        #region Properties

        [HttpGet("order/{id}/beam/properties")]
        public object Properties(int id)
        {
            var order = _dbContext.Orders.Where(e => e.Id == id)
                .Include(e => e.BeamInfo)
                .FirstOrDefault();

            if(order is null)
            {
                return NotFound();
            }

            var beam = order.BeamInfo;

            var warping_status = true;

            var warping = warping_status ?
                $"Iw Warping inertia = {Math.Round(beam.Warping, 0)} cm6" :
                "Iw Warping inertia cannot be calculated";

            var warpingRight = warping_status ?
                $"Iw Warping inertia = {Math.Round(beam.WarpingRight, 0)} cm6" :
                "Iw Warping inertia cannot be calculated";

            var selfWeight = beam.IsUniformDepth ? beam.SelfWeight : (beam.SelfWeight + beam.SelfWeightRight)/2;

            var surfArea = (beam.SurfAreaLeft + beam.SurfAreaRight) / 2;

            var surf = (beam.SurfPerLeft + beam.SurfPerRight)/2;

            var left = new List<string>()
            {
                $"Iy Major axis inertia = {Math.Round(beam.LeftInertia, 0)} cm4",
                $"Iz Minor axis inertia = {Math.Round(beam.MinorInertia/10000, 0)} cm4",
                $"iy Major axis gyration = {Math.Round(beam.IY, 1)} cm",
                $"iz Minor axis gyration = {Math.Round(beam.IZ, 1)} cm",
                $"It Torsional inertia = {Math.Round(beam.TorsConst, 1)} cm4",
                warping,
                $"A Cross section area = {Math.Round(beam.NoWebXSectionArea/100, 0)} cm2",
                $"Weight per m = {Math.Round(selfWeight, 0)} kg/m",
                $"Surface area per m = {Math.Round(surfArea, 1)} m2/m",
                $"Surface area per T = {Math.Round(surf, 1)} m2/T",
                "Ratio A/AQ = 1.15"
            };

            if (beam.IsUniformDepth)
            {
                return new
                {
                    left,
                    beam.IsUniformDepth
                };
            }

            var right = new List<string>()
            {
                $"Iy Major axis inertia = {Math.Round(beam.RightInertia, 0)} cm4",
                $"Iz Minor axis inertia = {Math.Round(beam.MinorInertia/10000, 0)} cm4",
                $"iy Major axis gyration = {Math.Round(beam.IYRight, 1)} cm",
                $"iz Minor axis gyration = {Math.Round(beam.IZRight, 1)} cm",
                $"It Torsional inertia = {Math.Round(beam.TorsConstRight, 1)} cm4",
                warpingRight,
                $"A Cross section area = {Math.Round(beam.NoWebXSectionArea/100, 0)} cm2"
            };

            return new
            {
                left,
                right,
                beam.IsUniformDepth
            };
        }

        #endregion

        #region Loading

        [HttpGet("order/{id}/beam/loading")]
        public object Loading(int id)
        {
            var order = _dbContext.Orders.Where(e => e.Id == id)
                .Include(e => e.BeamInfo)
                .Include(e => e.Loading)
                .ThenInclude(e => e.PointLoads)
                .FirstOrDefault();

            if (order is null)
            {
                return NotFound();
            }

            var beam = order.BeamInfo;

            var flanges_area = beam.TopFlangeThickness * beam.TopFlangeWidth +
                               beam.BottomFlangeThickness * beam.BottomFlangeWidth;

            var web_eff_thick = beam.WebThickness * 1;

            var ave_web_depth = 0.5 * (beam.WebDepth + beam.WebDepthRight);

            var section_area = flanges_area + web_eff_thick * ave_web_depth;

            var self_wt = Math.Round((section_area * 7850 * 9.81d / 1000000) / 1000, 2);

            var loading = order.Loading;

            LoadingDto loadingDto;

            if (loading is null)
            {
                loadingDto = new LoadingDto
                {
                    SelfWeight = self_wt,
                    LoadType = LoadType.CharacteristicLoads
                };
            }
            else
            {
                loadingDto = new LoadingDto
                {
                    SelfWeight = self_wt,
                    LoadType = loading.LoadType,
                    PointLoads = loading.PointLoads.Select(e=> new PointLoadDto
                    {
                        Id = e.Id,
                        Load = e.Load,
                        Position = e.Position
                    }).ToList(),
                    PermanentLoads = loading.LoadType == LoadType.CharacteristicLoads ? loading.PermanentLoads : null,
                    VariableLoads = loading.LoadType == LoadType.CharacteristicLoads ? loading.VariableLoads : null,
                    UltimateLoads = loading.LoadType == LoadType.UltimateLoads ? loading.UltimateLoads : null
                };
            }

            return loadingDto;
        }

        [HttpPost("order/{id}/beam/loading")]
        public IActionResult Loading(int id, LoadingDto loadingDto)
        {
            var order = _dbContext.Orders.Where(e => e.Id == id)
                .Include(e => e.Loading)
                .ThenInclude(e => e.PointLoads)
                .FirstOrDefault();

            if(order is null)
            {
                return NotFound();
            }

            if(order.Loading is null)
            {
                order.Loading = new Loading();
            }

            order.Loading.LoadType = loadingDto.LoadType;

            if(loadingDto.LoadType == LoadType.CharacteristicLoads)
            {
                order.Loading.PermanentLoads = loadingDto.PermanentLoads;
                order.Loading.VariableLoads = loadingDto.VariableLoads;
                order.Loading.UltimateLoads = null;
            }
            else
            {
                order.Loading.PermanentLoads = null;
                order.Loading.VariableLoads = null;
                order.Loading.UltimateLoads = loadingDto.UltimateLoads;
            }

            order.Loading.PointLoads?.Clear();

            order.Loading.PointLoads = new List<PointLoad>();

            foreach (var pointLoadDto in loadingDto.PointLoads)
            {
                order.Loading.PointLoads.Add(new PointLoad
                {
                    Position = pointLoadDto.Position,
                    Load = pointLoadDto.Load
                });
            }

            _dbContext.SaveChanges();

            return Ok();
        }

        #endregion

        #region Bending

        [HttpGet("order/{id}/analysis/bending")]
        public object Bending(int id)
        {
            var order = _dbContext.Orders.Where(e => e.Id == id)
                .Include(e => e.BeamInfo)
                .Include(e=>e.Localization)
                .Include(e => e.Loading)
                .ThenInclude(e => e.PointLoads)
                .FirstOrDefault();

            if(order is null)
            {
                return NotFound();
            }

            var beam = order.BeamInfo;
            var localization = order.Localization;
            var loading = order.Loading;


            var gamma_g = localization.DesignParameters.GammaG * localization.DesignParameters.ReductionFactorF;
            var gamma_q = localization.DesignParameters.GammaQ;

            var gamma_g_610a = localization.DesignParameters.GammaG;

            var gamma_q_610a = localization.DesignParameters.GammaQ * localization.PsiValue;

            if(loading.LoadType == LoadType.UltimateLoads)
            {
                gamma_g = 1;
                gamma_q = 0;
                loading.PermanentLoads = loading.UltimateLoads;
                loading.VariableLoads = new LoadParameters();
            }

            var applied_perm_udl = loading.PermanentLoads.Udl;

            var flanges_area = beam.TopFlangeThickness * beam.TopFlangeWidth +
                               beam.BottomFlangeThickness * beam.BottomFlangeWidth;

            var web_eff_thick = beam.WebThickness * 1;

            var ave_web_depth = 0.5 * (beam.WebDepth + beam.WebDepthRight);

            var section_area = flanges_area + web_eff_thick * ave_web_depth;

            var self_wt = Math.Round((section_area / (1000000) * 7850 * 9.81) / 1000, 2);

            var perm_udl = applied_perm_udl + self_wt;

            var vary_udl = loading.LoadType == LoadType.UltimateLoads ? 0 : loading.VariableLoads.Udl;

            double uls_udl_610b = 0;
            double sls_udl = 0;
            double uls_udl_610a = 0;
            double unfactored_uls = 0;

            if (perm_udl != 0 || vary_udl != 0)
            {
                uls_udl_610b = gamma_g * perm_udl + gamma_q * vary_udl;
                sls_udl = vary_udl;
                uls_udl_610a = gamma_g_610a * perm_udl + gamma_q_610a * vary_udl;
                unfactored_uls = perm_udl + vary_udl;
            }

            double uls_udl = 0;

            if (localization.ULSLoadExpression == ULSLoadExpression.Expression610a)
            {
                uls_udl = uls_udl_610b;
            }
            else
            {
                if(uls_udl_610a > 0)
                {
                    uls_udl = Math.Max(uls_udl_610a, uls_udl_610b);
                }
                else
                {
                    uls_udl = Math.Min(uls_udl_610a, uls_udl_610b);
                }
            }

            var partudl_perm = loading.PermanentLoads.PartialUdl;

            var partudl_vary = loading.LoadType == LoadType.UltimateLoads ? 0 : loading.VariableLoads.Udl;

            double part_uls_udl_610b = 0;
            double part_sls_udl = 0;
            double part_uls_udl_610a = 0;
            double part_unfactored_udl = 0;

            if (partudl_perm != 0 || partudl_vary != 0)
            {
                part_uls_udl_610b = gamma_g * partudl_perm + gamma_q * partudl_vary;
                part_sls_udl = partudl_vary;
                part_uls_udl_610a = gamma_g_610a * partudl_perm + gamma_q_610a * partudl_vary;
                part_unfactored_udl = partudl_perm + partudl_vary;
            }

            double part_uls_udl = 0;

            if (localization.ULSLoadExpression == ULSLoadExpression.Expression610a)
            {
                part_uls_udl = part_uls_udl_610b;
            }
            else
            {
                if (part_uls_udl > 0)
                {
                    part_uls_udl = Math.Max(part_uls_udl_610a, part_uls_udl_610b);
                }
                else
                {
                    part_uls_udl = Math.Min(part_uls_udl_610a, part_uls_udl_610b);
                }
            }

            var uls_left_mom_610b = gamma_g * loading.PermanentLoads.EndMomentLeft +
                                    gamma_q * loading.VariableLoads.EndMomentLeft;

            var uls_right_mom_610b = gamma_g * loading.PermanentLoads.EndMomentRight +
                                    gamma_q * loading.VariableLoads.EndMomentRight;

            var uls_left_mom_610a = gamma_g_610a * loading.PermanentLoads.EndMomentLeft +
                                    gamma_q_610a * loading.VariableLoads.EndMomentLeft;

            var uls_right_mom_610a = gamma_g_610a * loading.PermanentLoads.EndMomentRight + 
                                     gamma_q_610a * loading.VariableLoads.EndMomentRight;

            double uls_left_mom = 0;
            double uls_right_mom = 0;

            if (localization.ULSLoadExpression == ULSLoadExpression.Expression610a)
            {
                uls_left_mom = uls_left_mom_610b;
                uls_right_mom = uls_right_mom_610b;
            }
            else
            {
                if(uls_left_mom > 0)
                {
                    uls_left_mom = Math.Max(uls_left_mom_610a, uls_left_mom_610b);
                }
                else
                {
                    uls_left_mom = Math.Min(uls_left_mom_610a, uls_left_mom_610b);
                }

                if (uls_right_mom > 0)
                {
                    uls_right_mom = Math.Max(uls_right_mom_610a, uls_right_mom_610b);
                }
                else
                {
                    uls_right_mom = Math.Min(uls_right_mom_610a, uls_right_mom_610b);
                }
            }

            double unfactored_left_moment = 0;
            double unfactored_right_moment = 0;

            if (loading.LoadType == LoadType.UltimateLoads)
            {
                unfactored_left_moment = loading.PermanentLoads.EndMomentLeft;
                unfactored_right_moment = loading.PermanentLoads.EndMomentRight;
            }
            else
            {
                unfactored_left_moment = loading.PermanentLoads.EndMomentLeft + loading.VariableLoads.EndMomentLeft;
                unfactored_right_moment = loading.PermanentLoads.EndMomentRight + loading.VariableLoads.EndMomentRight;
            }

            double uls_axial_610b = gamma_g * loading.PermanentLoads.AxialForce + gamma_q * loading.VariableLoads.AxialForce;
            double uls_axial_610a = gamma_g_610a * loading.PermanentLoads.AxialForce + gamma_q_610a * loading.VariableLoads.AxialForce;

            double uls_axial = 0;

            if (loading.LoadType == LoadType.UltimateLoads)
            {
                uls_axial = uls_axial_610b;
            }
            else
            {
                if(uls_axial > 0)
                {
                    uls_axial = Math.Max(uls_axial_610a, uls_axial_610b);
                }
                else
                {
                    uls_axial = Math.Min(uls_axial_610a, uls_axial_610b);
                }
            }

            double unfactored_axial = 0;

            if (loading.LoadType == LoadType.UltimateLoads)
            {
                unfactored_axial = loading.PermanentLoads.AxialForce;
            }
            else
            {
                unfactored_axial = loading.PermanentLoads.AxialForce + loading.VariableLoads.AxialForce;
            }

            var sls_left_mom = loading.VariableLoads.EndMomentLeft;
            var sls_right_mom = loading.VariableLoads.EndMomentRight;

            double[,] arraypoints = new double[99, 99];

            for (int i = 0; i < loading.PointLoads.Count; i++)
            {
                if(localization.ULSLoadExpression == ULSLoadExpression.Expression610a)
                {
                    arraypoints[i, 4] = gamma_g * arraypoints[i, 2] + gamma_q * arraypoints[i, 3];
                }
                else
                {
                    if (arraypoints[i, 2] > 0)
                    {
                        arraypoints[i, 4] = Math.Max(gamma_g * arraypoints[i, 2] + gamma_q * arraypoints[i, 3],
                                                     gamma_g_610a * arraypoints[i, 2] + gamma_q_610a * arraypoints[i, 3]);
                    }
                    else
                    {
                        arraypoints[i, 4] = Math.Min(gamma_g * arraypoints[i, 2] + gamma_q * arraypoints[i, 3],
                             gamma_g_610a * arraypoints[i, 2] + gamma_q_610a * arraypoints[i, 3]);
                    }
                }
            }

            var part_udl_start = loading.PermanentLoads.PartialUdlStart;
            var part_udl_end = loading.PermanentLoads.PartialUdlEnd;

            var udl_moment = uls_udl * beam.Span * beam.Span / 2;
            var sls_udl_moment = sls_udl * beam.Span * beam.Span / 2;
            var unfactored_udl_moment = unfactored_uls * beam.Span * beam.Span / 2;

            var part_udl_moment = part_uls_udl * (part_udl_end - part_udl_start) * (beam.Span - 0.5 * (part_udl_start + part_udl_end));
            var part_sls_udl_moment = part_sls_udl * (part_udl_end - part_udl_start) * (beam.Span - 0.5 * (part_udl_start + part_udl_end));
            var unfactored_part_udl_moment = part_unfactored_udl * (part_udl_end - part_udl_start) * (beam.Span - 0.5 * (part_udl_start + part_udl_end));


            double points_moment = 0;
            double sls_points_moment = 0;
            double unfactored_points_moment = 0;

            for (int i = 0; i < loading.PointLoads.Count; i++)
            {
                var mom_contrib = arraypoints[i, 4] * (beam.Span - arraypoints[i, 1]);
                var sls_mom_contrib = arraypoints[i, 3] * (beam.Span - arraypoints[i, 1]);
                var unfactored_mom_contrib = (arraypoints[i, 2] + arraypoints[i, 3]) * (beam.Span - arraypoints[i, 1]);

                points_moment = points_moment + mom_contrib;
                sls_points_moment = sls_points_moment + sls_mom_contrib;
                unfactored_points_moment = unfactored_points_moment + unfactored_mom_contrib;
            }

            var lh_reaction = (udl_moment + points_moment + part_udl_moment - uls_right_mom - uls_left_mom) / beam.Span;
            var sls_lh_reaction = (sls_udl_moment + sls_points_moment + part_sls_udl_moment - sls_right_mom - sls_left_mom) / beam.Span;
            var unfactored_lh_reaction = (unfactored_udl_moment + unfactored_points_moment + unfactored_part_udl_moment - unfactored_left_moment - unfactored_right_moment) / beam.Span;

            var sheardef_sls_lh_reaction = (sls_udl_moment + sls_points_moment + part_sls_udl_moment) / beam.Span;
            var sheardef_unfactored_lh_reaction = (unfactored_udl_moment + unfactored_points_moment + unfactored_part_udl_moment) / beam.Span;

            const int segments = 100;
            var interval = beam.Span / segments;

            var bmdData = new double[segments + 3, 4];

            bmdData[0, 0] = 0;
            bmdData[0, 1] = uls_left_mom;

            double BM_part = 0;
            double sls_BM_part = 0;
            double unfactored_bm_part = 0;

            var momarray = new double[100, 100];

            for (int p = 1; p < segments; p++)
            {
                bmdData[p + 2, 0] = Math.Round(p * interval, 3);

                var BM_reaction = lh_reaction * p * interval + uls_left_mom;

                var sls_bm_reaction = sls_lh_reaction * p * interval + sls_left_mom;
                var unfactored_bm_reaction = unfactored_lh_reaction * p * interval + unfactored_left_moment;

                var sheardef_sls_bm_reaction = sls_lh_reaction * p * interval;
                var sheardef_unfactored_bm_reaction = unfactored_lh_reaction * p * interval;

                double BM_udl = -uls_udl * Math.Pow(p * interval, 2) * 0.5;
                double sls_bm_udl = -sls_udl * Math.Pow(p * interval, 2) * 0.5;
                double unfactored_bm_udl = -unfactored_uls * Math.Pow(p * interval, 2) * 0.5;

                if((p * interval) <= part_udl_start)
                {
                    BM_part = 0;
                    sls_BM_part = 0;
                    unfactored_bm_part = 0;
                }

                if (part_udl_start< (p* interval) && (p * interval) <= part_udl_end)
                {
                    BM_part = -part_uls_udl * Math.Pow((p * interval) - part_udl_start, 2) * 0.5;
                    sls_BM_part = -part_sls_udl * Math.Pow((p * interval) - part_udl_start, 2) * 0.5;
                    unfactored_bm_part = -part_unfactored_udl * Math.Pow((p * interval) - part_udl_start, 2) * 0.5;
                }

                if (part_udl_end< (p* interval))
                {
                    BM_part = -part_uls_udl * (part_udl_end - part_udl_start) * ((p * interval) - 0.5 * (part_udl_start + part_udl_end));
                    sls_BM_part = -part_sls_udl * (part_udl_end - part_udl_start) * ((p * interval) - 0.5 * (part_udl_start + part_udl_end));
                    unfactored_bm_part = -part_unfactored_udl * (part_udl_end - part_udl_start) * ((p * interval) - 0.5 * (part_udl_start + part_udl_end));
                }

                double BM_points = 0;
                double sls_bm_points = 0;
                double unfactored_bm_points = 0;

                for (int z = 0; z < loading.PointLoads.Count; z++)
                {
                    var lever = (p * interval) - arraypoints[z, 1];

                    if(lever > 0)
                    {
                        BM_points = BM_points - lever * arraypoints[z, 4];
                        sls_bm_points = sls_bm_points - lever * arraypoints[z, 3];
                        unfactored_bm_points = unfactored_bm_points - lever * (arraypoints[z, 2] + arraypoints[z, 3]);
                    }
                }

                var nett_bm = BM_reaction + BM_udl + BM_points + BM_part;
                var sls_nett_bm = sls_bm_reaction + sls_bm_udl + sls_bm_points + sls_BM_part;
                var unfactored_nett_bm = unfactored_bm_reaction + unfactored_bm_udl + unfactored_bm_points + unfactored_bm_part;

                bmdData[p + 2, 1] = Math.Round(p * interval, 3);

                momarray[p, 1] = nett_bm;
                momarray[p, 3] = sls_nett_bm;
                momarray[p, 50] = unfactored_nett_bm;
            }

            bmdData[segments + 2, 1] = Math.Pow(Math.Pow(beam.Span, 2), 0.5);
            bmdData[segments + 2, 2] = -uls_right_mom;

            double max_bm = int.MinValue;

            for (int i = 0; i < segments; i++)
            {
                var new_bm = momarray[i, 1];

                if(new_bm > max_bm)
                {
                    max_bm = new_bm;
                }
            }

            max_bm = new List<double> { max_bm, -uls_right_mom, uls_left_mom }.Max();

            double min_bm = int.MaxValue;

            for (int i = 0; i < segments; i++)
            {
                var new_bm = momarray[i, 1];

                if (new_bm < min_bm)
                {   
                    min_bm = new_bm;
                }
            }

            min_bm = new List<double> { min_bm, -uls_right_mom, uls_left_mom }.Min();


            return bmdData;
        }

        #endregion
    }

    public class BeamDto : Resource
    {
        public double Span { get; set; }
        public bool IsUniformDepth { get; set; } = true;
        public int WebDepthLeft { get; set; } = 1000;
        public int WebDepthRight { get; set; }
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
        public List<PointLoadDto> PointLoads { get; set; }
    }

    public class PointLoadDto
    {
        public int Id { get; set; }
        public double Position { get; set; }
        public double Load { get; set; }
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