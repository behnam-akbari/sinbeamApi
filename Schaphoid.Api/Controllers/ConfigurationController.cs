using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scaphoid.Core.Model;
using Scaphoid.Infrastructure.Data;
using System;
using System.Diagnostics.Metrics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

            orderDto.Links.Add(new Link("get-bending", Url.Action(nameof(Bending),
                null, new { id = id },
                Request.Scheme),
                HttpMethods.Get));

            orderDto.Links.Add(new Link("get-properties", Url.Action(nameof(Properties),
                null, new { id = id },
                Request.Scheme),
                HttpMethods.Get));

            orderDto.Links.Add(new Link("get-restraint", Url.Action(nameof(Restraints),
                null, new { id = id },
                Request.Scheme),
                HttpMethods.Get));

            orderDto.Links.Add(new Link("save-restraint", Url.Action(nameof(Restraints),
                null, new { id = id },
                Request.Scheme),
                HttpMethods.Post));

            orderDto.Links.Add(new Link("get-top-flange-verification", Url.Action(nameof(TopFlangeVerification),
                null, new { id = id },
                Request.Scheme),
                HttpMethods.Get));

            orderDto.Links.Add(new Link("get-bottom-flange-verification", Url.Action(nameof(BottomFlangeVerification),
                null, new { id = id },
                Request.Scheme),
                HttpMethods.Get));

            orderDto.Links.Add(new Link("get-web-verification", Url.Action(nameof(WebVerification),
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
                    Span = beam.Span,
                    SelfWeight = self_wt,
                    LoadType = LoadType.CharacteristicLoads
                };
            }
            else
            {
                loadingDto = new LoadingDto
                {
                    Span = beam.Span,
                    SelfWeight = self_wt,
                    LoadType = loading.LoadType,
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

        #endregion

        #region Bending

        [HttpGet("order/{id}/analysis/bending")]
        public object Bending(int id)
        {
            var order = _dbContext.Orders.Where(e => e.Id == id)
                .Include(e => e.BeamInfo)
                .Include(e => e.Localization)
                .Include(e => e.Loading)
                .ThenInclude(e => e.PointLoads)
                .FirstOrDefault();

            if (order is null)
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

            if (loading.LoadType == LoadType.UltimateLoads)
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

            beam.WebDepthRight = 1000;

            var ave_web_depth = 0.5 * (beam.WebDepth + beam.WebDepthRight);

            var section_area = flanges_area + web_eff_thick * ave_web_depth;

            var self_wt = Math.Round((section_area / (1000000) * 7850 * 9.81) / 1000, digits: 4);

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
                if (uls_udl_610a > 0)
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
                if (uls_left_mom > 0)
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
                if (uls_axial > 0)
                {
                    uls_axial = Math.Max(uls_axial_610a, uls_axial_610b);
                }
                else
                {
                    uls_axial = Math.Min(uls_axial_610a, uls_axial_610b);
                }
            }

            var ltbdata = new double[30];

            ltbdata[2] = uls_udl;
            ltbdata[3] = part_uls_udl;
            ltbdata[6] = uls_left_mom;
            ltbdata[7] = uls_right_mom;
            ltbdata[8] = uls_axial;

            var sls_left_mom = loading.VariableLoads.EndMomentLeft;
            var sls_right_mom = loading.VariableLoads.EndMomentRight;

            double[,] arraypoints = GetArrayPoints(localization, loading, gamma_g, gamma_q, gamma_g_610a, gamma_q_610a);

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

            ltbdata[9] = lh_reaction;

            var sls_lh_reaction = (sls_udl_moment + sls_points_moment + part_sls_udl_moment - sls_right_mom - sls_left_mom) / beam.Span;
            var unfactored_lh_reaction = (unfactored_udl_moment + unfactored_points_moment + unfactored_part_udl_moment - unfactored_left_moment - unfactored_right_moment) / beam.Span;

            var sheardef_sls_lh_reaction = (sls_udl_moment + sls_points_moment + part_sls_udl_moment) / beam.Span;
            var sheardef_unfactored_lh_reaction = (unfactored_udl_moment + unfactored_points_moment + unfactored_part_udl_moment) / beam.Span;

            const int segments = 100;
            var interval = beam.Span / segments;

            var bmdData = new double[segments + 1, 4];

            bmdData[0, 0] = 0;
            bmdData[0, 1] = uls_left_mom;

            double BM_part = 0;
            double sls_BM_part = 0;
            double unfactored_bm_part = 0;

            var momarray = new double[100, 100];

            for (int p = 1; p < segments; p++)
            {
                bmdData[p, 0] = Math.Round(p * interval, 3);

                var BM_reaction = lh_reaction * p * interval + uls_left_mom;

                var sls_bm_reaction = sls_lh_reaction * p * interval + sls_left_mom;
                var unfactored_bm_reaction = unfactored_lh_reaction * p * interval + unfactored_left_moment;

                var sheardef_sls_bm_reaction = sls_lh_reaction * p * interval;
                var sheardef_unfactored_bm_reaction = unfactored_lh_reaction * p * interval;

                double BM_udl = -uls_udl * Math.Pow(p * interval, 2) * 0.5;
                double sls_bm_udl = -sls_udl * Math.Pow(p * interval, 2) * 0.5;
                double unfactored_bm_udl = -unfactored_uls * Math.Pow(p * interval, 2) * 0.5;

                if ((p * interval) <= part_udl_start)
                {
                    BM_part = 0;
                    sls_BM_part = 0;
                    unfactored_bm_part = 0;
                }

                if (part_udl_start < (p * interval) && (p * interval) <= part_udl_end)
                {
                    BM_part = -part_uls_udl * Math.Pow((p * interval) - part_udl_start, 2) * 0.5;
                    sls_BM_part = -part_sls_udl * Math.Pow((p * interval) - part_udl_start, 2) * 0.5;
                    unfactored_bm_part = -part_unfactored_udl * Math.Pow((p * interval) - part_udl_start, 2) * 0.5;
                }

                if (part_udl_end < (p * interval))
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

                    if (lever > 0)
                    {
                        BM_points = BM_points - lever * arraypoints[z, 4];
                        sls_bm_points = sls_bm_points - lever * arraypoints[z, 3];
                        unfactored_bm_points = unfactored_bm_points - lever * (arraypoints[z, 2] + arraypoints[z, 3]);
                    }
                }

                var nett_bm = BM_reaction + BM_udl + BM_points + BM_part;
                var sls_nett_bm = sls_bm_reaction + sls_bm_udl + sls_bm_points + sls_BM_part;
                var unfactored_nett_bm = unfactored_bm_reaction + unfactored_bm_udl + unfactored_bm_points + unfactored_bm_part;

                bmdData[p, 1] = nett_bm;//Math.Round(p * interval, 3);

                momarray[p, 1] = nett_bm;
                momarray[p, 3] = sls_nett_bm;
                momarray[p, 50] = unfactored_nett_bm;
            }

            bmdData[segments, 0] = Math.Pow(Math.Pow(beam.Span, 2), 0.5);
            bmdData[segments, 1] = -uls_right_mom;

            double max_bm = int.MinValue;

            for (int i = 0; i < segments; i++)
            {
                var new_bm = momarray[i, 1];

                if (new_bm > max_bm)
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


            var bendingPoints = new List<Point>();

            for (int i = 0; i < segments + 1; i++)
            {
                bendingPoints.Add(new Point(bmdData[i, 0], bmdData[i, 1]));
            }

            /////////////////////////////////////////////////////////////////////////////////
            ///

            bmdData[0, 2] = Math.Round(lh_reaction, 3);

            for (int p = 1; p < segments; p++)
            {
                double shear_udl = -uls_udl * (p * interval);
                double sls_shear_udl = -sls_udl * (p * interval);
                double unfactored_shear_udl = -unfactored_uls * (p * interval);

                double shear_points = 0;
                double sls_shear_points = 0;
                double unfactored_shear_points = 0;

                for (int z = 0; z < loading.PointLoads.Count; z++)
                {
                    var lever = (p * interval) - arraypoints[z, 1];

                    if (lever > 0)
                    {
                        shear_points = shear_points - arraypoints[z, 4];
                        sls_shear_points = sls_shear_points - arraypoints[z, 3];
                        unfactored_shear_points = unfactored_shear_points - (arraypoints[z, 2] + arraypoints[z, 3]);
                    }
                }

                double shear_part = 0;
                double sls_shear_part = 0;
                double unfactored_shear_part = 0;

                if ((p * interval) <= part_udl_start)
                {
                    shear_part = 0;
                    sls_shear_part = 0;
                    unfactored_shear_part = 0;
                }

                if (part_udl_start < (p * interval) && (p * interval) <= part_udl_end)
                {
                    shear_part = -part_uls_udl * ((p * interval) - part_udl_start);
                    sls_shear_part = -part_sls_udl * ((p * interval) - part_udl_start);
                    unfactored_shear_part = -part_unfactored_udl * ((p * interval) - part_udl_start);
                }

                if (part_udl_end < (p * interval))
                {
                    shear_part = -part_uls_udl * (part_udl_end - part_udl_start);
                    sls_shear_part = -part_sls_udl * (part_udl_end - part_udl_start);
                    unfactored_shear_part = -part_unfactored_udl * (part_udl_end - part_udl_start);
                }

                double nett_shear = lh_reaction + shear_udl + shear_points + shear_part;
                double sls_nett_shear = sls_lh_reaction + sls_shear_udl + sls_shear_points + sls_shear_part;
                double unfactored_nett_shear = unfactored_lh_reaction + unfactored_shear_udl +
                                            unfactored_shear_points + unfactored_shear_part;

                double sheardef_sls_nett_shear = sheardef_sls_lh_reaction + sls_shear_udl + sls_shear_points + sls_shear_part;
                double sheardef_unfactored_nett_shear = sheardef_unfactored_lh_reaction + unfactored_shear_udl +
                                                        unfactored_shear_points + unfactored_shear_part;

                bmdData[p, 2] = nett_shear;

                momarray[p, 2] = nett_shear;
                momarray[p, 44] = sls_nett_shear;
                momarray[p, 49] = unfactored_nett_shear;
                momarray[p, 60] = sheardef_sls_nett_shear;
                momarray[p, 61] = sheardef_unfactored_nett_shear;
            }

            double total_points_load = 0;

            for (int i = 0; i < loading.PointLoads.Count; i++)
            {
                total_points_load = total_points_load + arraypoints[i, 4];
            }

            double total_partial_udl = ltbdata[3] * (part_udl_end - part_udl_start);
            double rh_reaction = lh_reaction - beam.Span * uls_udl - total_partial_udl - total_points_load;

            bmdData[segments, 2] = rh_reaction;
            bmdData[segments, 1] = Math.Pow(Math.Pow(beam.Span, 2), .5);
            bmdData[segments, 3] = 0;

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////

            var deflection_positions = new double[10];

            for (int i = 1; i < deflection_positions.Length; i++)
            {
                deflection_positions[i] = i * beam.Span / 10;
            }

            for (int i = 1; i < deflection_positions.Length; i++)
            {
                double lh_unit_reaction = (beam.Span - deflection_positions[i]) / beam.Span;

                for (int j = 1; j < segments - 1; j++)
                {
                    double lever = (j * interval) - deflection_positions[i];

                    double unit_lever = lever > 0 ? lever : 0;

                    double unit_moment = lh_unit_reaction * (j * interval) - unit_lever;

                    momarray[j, i + 3] = unit_moment;
                }
            }

            beam.WebDepthRight = 500;

            double leftheight = Math.Pow(Math.Pow(beam.WebDepth, 2), 0.5);
            double rightheight = Math.Pow(Math.Pow(beam.WebDepthRight, 2), 0.5);

            for (int j = 1; j < segments - 1; j++)
            {
                var section_position = j * interval;
                var section_depth = leftheight - (leftheight - rightheight) * section_position / beam.Span;
                momarray[j, 13] = section_depth;
            }

            for (int j = 1; j < segments - 1; j++)
            {
                momarray[j, 25] = momarray[j, 13] + 0.5 * (beam.TopFlangeThickness + beam.BottomFlangeThickness);
            }

            double top_flg_thick = beam.TopFlangeThickness;
            double top_flg_width = beam.TopFlangeWidth;
            double bottom_flg_thick = beam.BottomFlangeThickness;
            double bottom_flg_width = beam.BottomFlangeWidth;
            double web = beam.WebThickness;

            for (int j = 1; j < segments - 1; j++)
            {
                double web_depth = momarray[j, 13];
                section_area = top_flg_thick * top_flg_width + bottom_flg_thick * bottom_flg_width;
                double n_axis = (bottom_flg_width * bottom_flg_thick * bottom_flg_thick * 0.5 +
                    top_flg_thick * top_flg_width * (bottom_flg_thick + web_depth + 0.5 * top_flg_thick)) / section_area;

                double top_flg_inertia = top_flg_width * Math.Pow(top_flg_thick, 3) / 12 +
                                         top_flg_width * top_flg_thick * Math.Pow(bottom_flg_thick + web_depth + 0.5 * top_flg_thick - n_axis, 2);

                double bot_flg_inertia = bottom_flg_width * Math.Pow(bottom_flg_thick, 3) / 12 +
                                         bottom_flg_width * bottom_flg_thick * Math.Pow((0.5 * bottom_flg_thick - n_axis), 2);

                double sec_inertia = top_flg_inertia + bot_flg_inertia;

                momarray[j, 14] = sec_inertia;

                momarray[j, 41] = (momarray[j, 60] * 1000 / (web * web_depth)) * (178 / 155d);
                momarray[j, 48] = (momarray[j, 61] * 1000 / (web * web_depth)) * (178 / 155d);
                momarray[j, 42] = momarray[j, 41] * interval * 1000 / 81000d;
                momarray[j, 47] = momarray[j, 48] * interval * 1000 / 81000d;
            }

            for (int j = 1; j < segments - 1; j++)
            {
                momarray[j, 15] = interval * momarray[j, 3] * momarray[j, 4] / momarray[j, 14];
                momarray[j, 16] = interval * momarray[j, 3] * momarray[j, 5] / momarray[j, 14];
                momarray[j, 17] = interval * momarray[j, 3] * momarray[j, 6] / momarray[j, 14];
                momarray[j, 18] = interval * momarray[j, 3] * momarray[j, 7] / momarray[j, 14];
                momarray[j, 19] = interval * momarray[j, 3] * momarray[j, 8] / momarray[j, 14];
                momarray[j, 20] = interval * momarray[j, 3] * momarray[j, 9] / momarray[j, 14];
                momarray[j, 21] = interval * momarray[j, 3] * momarray[j, 10] / momarray[j, 14];
                momarray[j, 22] = interval * momarray[j, 3] * momarray[j, 11] / momarray[j, 14];
                momarray[j, 23] = interval * momarray[j, 3] * momarray[j, 12] / momarray[j, 14];


                momarray[j, 51] = interval * momarray[j, 50] * momarray[j, 4] / momarray[j, 14];
                momarray[j, 52] = interval * momarray[j, 50] * momarray[j, 5] / momarray[j, 14];
                momarray[j, 53] = interval * momarray[j, 50] * momarray[j, 6] / momarray[j, 14];
                momarray[j, 54] = interval * momarray[j, 50] * momarray[j, 7] / momarray[j, 14];
                momarray[j, 55] = interval * momarray[j, 50] * momarray[j, 8] / momarray[j, 14];
                momarray[j, 56] = interval * momarray[j, 50] * momarray[j, 9] / momarray[j, 14];
                momarray[j, 57] = interval * momarray[j, 50] * momarray[j, 10] / momarray[j, 14];
                momarray[j, 58] = interval * momarray[j, 50] * momarray[j, 11] / momarray[j, 14];
                momarray[j, 59] = interval * momarray[j, 50] * momarray[j, 12] / momarray[j, 14];
            }

            var def_sums = new double[12, 11];

            def_sums[1, 4] = 0;
            def_sums[11, 4] = 0;
            def_sums[1, 5] = 0;
            def_sums[11, 5] = 0;

            for (int k = 0; k <= 9; k++)
            {
                double shear_def = 0;
                double unfactored_shear_def = 0;
                double distance = k * beam.Span / 10;

                for (int j = 0; j < segments - 1; j++)
                {
                    if (j * interval < distance)
                    {
                        shear_def = shear_def + momarray[j, 42];
                        unfactored_shear_def = unfactored_shear_def + momarray[j, 47];
                    }
                }

                def_sums[k + 1, 4] = shear_def;
                def_sums[k + 1, 10] = unfactored_shear_def;
            }

            for (int k = 1; k <= 10; k++)
            {
                double defl_coeff = 0;
                double unfactored_defl_coeff = 0;

                for (int j = 0; j < segments - 1; j++)
                {
                    defl_coeff = defl_coeff + momarray[j, k + 13];
                    unfactored_defl_coeff = unfactored_defl_coeff + momarray[j, k + 49];
                }

                def_sums[k, 2] = defl_coeff * 1000000000 / 210d;
                def_sums[k, 6] = unfactored_defl_coeff * 1000000000 / 210d;
            }

            def_sums[1, 2] = 0;
            def_sums[11, 1] = Math.Pow(Math.Pow(beam.Span, 2), 0.5);
            def_sums[1, 1] = 0;
            def_sums[11, 2] = 0;
            def_sums[1, 6] = 0;
            def_sums[11, 6] = 0;

            for (int k = 1; k <= 10; k++)
            {
                def_sums[k, 1] = deflection_positions[k - 1];
                def_sums[k, 5] = def_sums[k, 2] + def_sums[k, 4];
                def_sums[k, 7] = def_sums[k, 6] + def_sums[k, 10];
            }

            var sheerDeflectionPoints = new List<Point>();
            var bendingDeflectionPoints = new List<Point>();
            var totalLoadDeflectionPoints = new List<Point>();

            for (int i = 1; i <= 11; i++)
            {
                var sheerDeflectionX = def_sums[i, 1];
                var sheerDeflectionY = def_sums[i, 10];
                var bendingDeflectionY = def_sums[i, 6];
                var totalLoadDeflectionY = def_sums[i, 7];

                sheerDeflectionPoints.Add(new Point(sheerDeflectionX, Math.Round(sheerDeflectionY, 2)));
                bendingDeflectionPoints.Add(new Point(sheerDeflectionX, Math.Round(bendingDeflectionY, 2)));
                totalLoadDeflectionPoints.Add(new Point(sheerDeflectionX, Math.Round(totalLoadDeflectionY, 2)));
            }

            var sheerPoints = new List<Point>();

            for (int i = 0; i < segments + 1; i++)
            {
                sheerPoints.Add(new Point(bmdData[i, 0], Math.Round(bmdData[i, 2], 2)));
            }

            return new
            {
                bending = new
                {
                    points = bendingPoints,
                    maxMoment = max_bm,
                    minMoment = min_bm,
                },
                shear = new
                {
                    points = sheerPoints,
                    MaxShear = sheerPoints.Select(e => e.Y).Max(),
                    MinShear = sheerPoints.Select(e => e.Y).Min(),
                },
                deflection = new
                {
                    sheer = sheerDeflectionPoints,
                    bending = bendingDeflectionPoints,
                    total = totalLoadDeflectionPoints,
                    MaxDefln = totalLoadDeflectionPoints.Select(e => e.Y).Max()
                }
            };
        }

        private static double[,] GetArrayPoints(Localization localization, Loading loading, double gamma_g, double gamma_q, double gamma_g_610a, double gamma_q_610a)
        {
            double[,] arraypoints = new double[99, 99];

            for (int i = 0; i < loading.PointLoads.Count; i++)
            {
                if (localization.ULSLoadExpression == ULSLoadExpression.Expression610a)
                {
                    arraypoints[i, 1] = loading.PointLoads.ToArray()[i].Position; // To Do

                    arraypoints[i, 2] = loading.PointLoads.ToArray()[i].Load; // To Do

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

            return arraypoints;
        }

        #endregion

        #region Restraints

        [HttpGet("order/{id}/restraints")]
        public object Restraints(int id)
        {
            var order = _dbContext.Orders.Where(e => e.Id == id)
                .Include(e => e.BeamInfo)
                .Include(e => e.Restraint)
                .FirstOrDefault();

            if(order is null)
            {
                return NotFound();
            }

            var beam = order.BeamInfo;

            if(order.Restraint is null)
            {
                var restraintDto = new RestraintDto
                {
                    TopFlangeRestraints = new List<double> { 0, beam.Span },
                    BottomFlangeRestraints = new List<double> { 0, beam.Span },
                    FullRestraintTopFlange = false, 
                    FullRestraintBottomFlange = false
                };

                return restraintDto;
            }
            else
            {
                var restraint = order.Restraint;

                var restraintDto = new RestraintDto
                {
                    TopFlangeRestraints = restraint.TopFlangeRestraints,
                    BottomFlangeRestraints = restraint.BottomFlangeRestraints,
                    FullRestraintTopFlange = restraint.FullRestraintTopFlange,
                    FullRestraintBottomFlange = restraint.FullRestraintBottomFlange
                };

                return restraintDto;
            }
        }

        [HttpPost("order/{id}/restraints")]
        public IActionResult Restraints(int id, RestraintDto restraintDto)
        {
            var order = _dbContext.Orders.Where(e => e.Id == id)
                .Include(e => e.Restraint)
                .FirstOrDefault();

            if (order is null)
            {
                return NotFound();
            }

            order.Restraint = new Restraint
            {
                BottomFlangeRestraints = restraintDto.BottomFlangeRestraints,
                TopFlangeRestraints = restraintDto.TopFlangeRestraints,
                FullRestraintBottomFlange = restraintDto.FullRestraintBottomFlange,
                FullRestraintTopFlange = restraintDto.FullRestraintTopFlange
            };

            _dbContext.SaveChanges();

            return Ok();
        }

        #endregion

        #region Flange Verification

        [HttpGet("order/{id}/top-flange-verification")]
        public object TopFlangeVerification(int id)
        {
            var order = _dbContext.Orders.Where(e => e.Id == id)
                .Include(e => e.BeamInfo)
                .Include(e => e.Restraint)
                .Include(e => e.Loading)
                .ThenInclude(e => e.PointLoads)
                .Include(e => e.Localization)
                .FirstOrDefault();

            if (order is null)
            {
                return NotFound();
            }

            var top_ute_condition = string.Empty;
            var beam = order.BeamInfo;
            var localization = order.Localization;
            var loading = order.Loading;

            const int segments = 100;
            var interval = beam.Span / segments;

            var depth_left = beam.WebDepth;

            var depth_right = beam.IsUniformDepth ? depth_left : beam.WebDepthRight;

            double leftheight = Math.Pow(Math.Pow(beam.WebDepth, 2), 0.5);
            double rightheight = Math.Pow(Math.Pow(beam.WebDepthRight, 2), 0.5);

            var gamma_g = localization.DesignParameters.GammaG * localization.DesignParameters.ReductionFactorF;
            var gamma_q = localization.DesignParameters.GammaQ;

            var momarray = new double[100, 100];

            for (int j = 1; j < segments - 1; j++)
            {
                var section_position = j * interval;
                var section_depth = leftheight - (leftheight - rightheight) * section_position / beam.Span;
                momarray[j, 13] = section_depth;
            }

            var restraint = order.Restraint;

            restraint.TopFlangeRestraints = restraint.TopFlangeRestraints.OrderBy(e => e).ToList();

            var ltbtop = new double[100, 50];

            if (restraint.FullRestraintTopFlange == false)
            {
                for (int i = 1; i <= restraint.TopFlangeRestraints.Count; i++)
                {
                    ltbtop[i, 1] = restraint.TopFlangeRestraints[i - 1];
                }

                ltbtop[1, 2] = 0;

                for (int i = 1; i < restraint.TopFlangeRestraints.Count; i++)
                {
                    ltbtop[i + 1, 2] = ltbtop[i + 1, 1] - ltbtop[i, 1];
                }

                for (int i = 1; i <= restraint.TopFlangeRestraints.Count; i++)
                {
                    var distance = ltbtop[i, 1];

                    var nett_bm = bmoment(order, distance);

                    ltbtop[i, 3] = nett_bm;
                }

                for (int i = 1; i <= restraint.TopFlangeRestraints.Count; i++)
                {
                    for (int k = 1; k < 3; k++)
                    {
                        var distance = ltbtop[i, 1] + k * ltbtop[i + 1, 2] * 0.25;
                        var nett_bm = bmoment(order, distance);
                        ltbtop[i + 1, k + 4] = nett_bm;
                    }
                }

                for (int i = 1; i <= restraint.TopFlangeRestraints.Count; i++)
                {
                    double momstart = ltbtop[i, 3];
                    double mom_25 = ltbtop[i + 1, 5];
                    double mom_5 = ltbtop[i + 1, 6];
                    double mom_75 = ltbtop[i + 1, 7];
                    double momend = ltbtop[i + 1, 3];

                    double segment_max = new List<double> { momstart, mom_25, mom_5, mom_75, momend }.Max();
                    double segment_min = new List<double> { momstart, mom_25, mom_5, mom_75, momend }.Max();

                    if (Math.Pow(segment_max, 2) < Math.Pow(segment_min, 2))
                    {
                        momstart = -1 * momstart;
                        mom_25 = -1 * mom_25;
                        mom_5 = -1 * mom_5;
                        mom_75 = -1 * mom_75;
                        momend = -1 * momend;
                        ltbtop[i + 1, 8] = segment_min;
                        segment_max = -1 * segment_min;
                    }
                    else
                    {
                        ltbtop[i + 1, 8] = segment_max;
                    }

                    var c_one = Math.Pow((35 * Math.Pow(segment_max, 2) / (Math.Pow(segment_max, 2) + 9 * Math.Pow(mom_25, 2) + 16 * Math.Pow(mom_5, 2) + 9 * Math.Pow(mom_75, 2))), 0.5);

                    ltbtop[i + 1, 9] = c_one;
                    ltbtop[i + 1, 10] = Math.Pow(c_one, -0.5);
                }

                //var steeltop = beam.TopFlangeSteel == SteelType.S355 ? 355 : 275;
                var steeltop = 355;

                var top_flg_thick = beam.TopFlangeThickness;
                var top_flg_width = beam.TopFlangeWidth;
                var bottom_flg_thick = beam.BottomFlangeThickness;
                var bottom_flg_width = beam.BottomFlangeWidth;

                var top_area = top_flg_thick * top_flg_width;
                var bottom_area = bottom_flg_thick * bottom_flg_width;

                var topstrength = designstrength(order, top_flg_thick, steeltop);

                var epsilon_top = Math.Pow((235 / topstrength), 0.5);
                var lambda_one_top = 93.9 * epsilon_top;

                var span = beam.Span;
                double beta = 0;

                if (span * 1000 / (top_flg_width * 0.5) > 50)
                {
                    beta = 1;
                }
                else
                {
                    var kappa = 0.5 * top_flg_width / (span * 1000);

                    if (kappa <= 0.02)
                    {
                        beta = 1;
                    }
                    else if (0.02 < kappa && kappa <= 0.7)
                    {
                        beta = 1 / (1 + 6.4 * Math.Pow(kappa, 2));
                    }
                    else if (kappa > 0.7)
                    {
                        beta = 1 / (5.9 * kappa);
                    }
                }

                var psi_top = 1.25 * (beta - 0.2);

                var k_sigma = psi_top == 1 ? 0.43 : 0.578 / (psi_top + 0.34);

                var b_bar = 0.5 * (top_flg_width - 0.5 * 40);

                var lambda_p = (b_bar / top_flg_thick) / (28.4 * epsilon_top * Math.Pow(k_sigma, 0.5));

                var rho_top = lambda_p <= 0.748 ? 1 : Math.Min(1, (lambda_p - 0.188) / Math.Pow(lambda_p, 2));

                var aeff_top = rho_top * top_flg_thick * top_flg_width;
                var aeff_top_tension = beta * top_flg_thick * top_flg_width;

                var top_flange_tension_resi = aeff_top_tension * topstrength / 1000;

                var gyra_top = Math.Pow(top_flg_thick * Math.Pow(top_flg_width, 3) / 12 / (top_flg_thick * top_flg_width), 0.5);

                var alpha = top_flg_thick <= 40 ? 0.49 : 0.76;

                for (int i = 2; i <= restraint.TopFlangeRestraints.Count; i++)
                {
                    ltbtop[i, 11] = (ltbtop[i, 1] - ltbtop[i - 1, 1]) * 1000;
                    ltbtop[i, 12] = ltbtop[i, 10] * ltbtop[i, 11] / (gyra_top * lambda_one_top);
                    ltbtop[i, 13] = 0.5 * (1 + alpha * (ltbtop[i, 12] - 0.2) + Math.Pow(ltbtop[i, 12], 2));
                    ltbtop[i, 14] = Math.Min(1, 1 / (ltbtop[i, 13] + Math.Pow((Math.Pow(ltbtop[i, 13], 2) - Math.Pow(ltbtop[i, 12], 2)), 0.5)));
                    ltbtop[i, 15] = aeff_top * ltbtop[i, 14] * topstrength / 1000;
                }

                for (int i = 1; i <= restraint.TopFlangeRestraints.Count; i++)
                {
                    ltbtop[i, 16] = depth_left - (depth_left - depth_right) * (ltbtop[i, 1] / span);
                }

                var extra = 0.5 * (top_flg_thick + bottom_flg_thick);

                for (int i = 2; i <= restraint.TopFlangeRestraints.Count; i++)
                {
                    ltbtop[i, 17] = ltbtop[i - 1, 3] / ((ltbtop[i - 1, 16] + 0.5 * (top_flg_thick + bottom_flg_thick)) / 1000);
                    ltbtop[i, 18] = ltbtop[i, 3] / ((ltbtop[i, 16] + 0.5 * (top_flg_thick + bottom_flg_thick)) / 1000);

                    var max_segment_force = double.MinValue;

                    for (int g = 1; g < segments - 1; g++)
                    {
                        if ((g * interval) >= ltbtop[i - 1, 1] && g * interval <= ltbtop[i, 1])
                        {
                            double force_at_g = momarray[g, 1] / ((momarray[g, 13] + extra) / 1000);

                            if (force_at_g > max_segment_force)
                            {
                                max_segment_force = force_at_g;
                            }
                        }
                    }

                    var min_segment_force = double.MaxValue;

                    for (int g = 1; g < segments - 1; g++)
                    {
                        if ((g * interval) >= ltbtop[i - 1, 1] && g * interval <= ltbtop[i, 1])
                        {
                            double force_at_g = momarray[g, 1] / ((momarray[g, 13] + extra) / 1000);

                            if (force_at_g < min_segment_force)
                            {
                                min_segment_force = force_at_g;
                            }
                        }
                    }

                    ltbtop[i, 25] = min_segment_force;
                    ltbtop[i, 20] = max_segment_force;
                    ltbtop[i, 19] = new List<double> { ltbtop[i, 17], ltbtop[i, 18], ltbtop[i, 20] }.Max();
                    ltbtop[i, 27] = new List<double> { ltbtop[i, 25], ltbtop[i, 17], ltbtop[i, 18] }.Min();
                }

                var axial_perm = loading.PermanentLoads.AxialForce;
                var axial_vary = loading.VariableLoads.AxialForce;

                var ult_axial = gamma_g * axial_perm + gamma_q * axial_vary;
                var top_axial_force = ult_axial * top_area / (bottom_area + top_area);

                for (int i = 2; i <= restraint.TopFlangeRestraints.Count; i++)
                {
                    ltbtop[i, 21] = ltbtop[i, 27] + top_axial_force;
                    ltbtop[i, 26] = ltbtop[i, 19] + top_axial_force;

                    if(ltbtop[i, 21] <= 0 && ltbtop[i, 26] <= 0)
                    {
                        ltbtop[i, 29] = -1;
                    }
                    else if(ltbtop[i, 21] >= 0 && ltbtop[i, 26] >= 0)
                    {
                        ltbtop[i, 29] = 1;
                    }
                    else
                    {
                        ltbtop[i, 29] = 99;
                    }

                    if (ltbtop[i, 29] == -1)
                    {
                        ltbtop[i, 22] = -ltbtop[i, 21] / top_flange_tension_resi;
                    }
                    else if(ltbtop[i, 29] == 1)
                    {
                        ltbtop[i, 23] = ltbtop[i, 26] / ltbtop[i, 15];
                    }
                    else
                    {
                        ltbtop[i, 22] = -ltbtop[i, 21] / top_flange_tension_resi;
                        ltbtop[i, 23] = ltbtop[i, 26] / ltbtop[i, 15];

                        ltbtop[i, 30] = Math.Max(ltbtop[i, 22], ltbtop[i, 23]);

                        top_ute_condition = ltbtop[i, 30] == ltbtop[i, 22] ? "tension" : "compression";
                    }
                }

                double max_top_ute = 0;
                var max_position = 0;

                for (int i = 2; i <= restraint.TopFlangeRestraints.Count; i++)
                {
                    var top_ute = Math.Max(ltbtop[i, 22], ltbtop[i, 23]);

                    if(top_ute > max_top_ute)
                    {
                        max_top_ute = top_ute;
                        max_position = i;
                    }
                }

                var verificationItems = new List<VerificationItem>();

                for (int i = 2; i <= restraint.TopFlangeRestraints.Count; i++)
                {
                    var verificationItem = new VerificationItem
                    {
                        From = Math.Round(ltbtop[i - 1, 1], 3),
                        To = Math.Round(ltbtop[i, 1], 3)
                    };

                    if(ltbtop[i, 29] == -1)
                    {
                        verificationItem.DesignForce = Math.Round(ltbtop[i, 21], 0);
                        verificationItem.Resistance = Math.Round(top_flange_tension_resi, 0);
                        verificationItem.Utilization = Math.Round(ltbtop[i, 22], 2);
                    }
                    else if (ltbtop[i, 29] == 1)
                    {
                        verificationItem.DesignForce = Math.Round(ltbtop[i, 26], 0);
                        verificationItem.Resistance = Math.Round(ltbtop[i, 15], 0);
                        verificationItem.Utilization = Math.Round(ltbtop[i, 23], 2);
                    }
                    else
                    {
                        if(ltbtop[i, 30] == ltbtop[i, 22])
                        {
                            verificationItem.DesignForce = Math.Round(ltbtop[i, 21], 0);
                            verificationItem.Resistance = Math.Round(top_flange_tension_resi, 0);
                            verificationItem.Utilization = Math.Round(ltbtop[i, 22], 2);
                        }
                        else
                        {
                            verificationItem.DesignForce = Math.Round(ltbtop[i, 26], 0);
                            verificationItem.Resistance = Math.Round(ltbtop[i, 15], 0);
                            verificationItem.Utilization = Math.Round(ltbtop[i, 23], 2);
                        }
                    }

                    verificationItem.Captions = GetTopFlangePositionCaptions(ltbtop, aeff_top, top_flange_tension_resi, top_axial_force, i);

                    verificationItems.Add(verificationItem);
                }

                var captions = max_position > 0 ?
                    GetTopFlangeMaxPositionCaptions(top_ute_condition, ltbtop, aeff_top, top_flange_tension_resi, top_axial_force, max_position) :
                    new List<string>();

                return new
                {
                    MaximumUtilization = Math.Round(max_top_ute, 2),
                    verificationItems,
                    captions
                };
            }
            else
            {
                var steeltop = beam.TopFlangeSteel == SteelType.S355 ? 355 : 275;

                var bottom_flg_thick = beam.BottomFlangeThickness;
                var bottom_flg_width = beam.BottomFlangeWidth;
                var area_bottom = bottom_flg_thick * bottom_flg_width;

                var top_flg_thick = beam.TopFlangeThickness;
                var top_flg_width = beam.TopFlangeWidth;
                var area_top = top_flg_thick * top_flg_width;

                var topstrength = designstrength(order, top_flg_thick, steeltop);

                var span = beam.Span;
                double beta = 0;

                if (span * 1000 / (top_flg_width * 0.5) > 50)
                {
                    beta = 1;
                }
                else
                {
                    var kappa = 0.5 * top_flg_width / (span * 1000);

                    if (kappa <= 0.02)
                    {
                        beta = 1;
                    }
                    else if (0.02 < kappa && kappa <= 0.7)
                    {
                        beta = 1 / (1 + 6.4 * Math.Pow(kappa, 2));
                    }
                    else if (kappa > 0.7)
                    {
                        beta = 1 / (5.9 * kappa);
                    }
                }

                var aeff_top = beta * top_flg_thick * top_flg_width;

                var resi_top = aeff_top * topstrength / 1000;

                if (loading.PermanentLoads is null)
                {
                    loading.PermanentLoads = new LoadParameters();
                }

                if (loading.VariableLoads is null)
                {
                    loading.VariableLoads = new LoadParameters();
                }

                var axial_perm = loading.PermanentLoads.AxialForce;
                var axial_vary = loading.VariableLoads.AxialForce;

                var ult_axial = gamma_g * axial_perm + gamma_q * axial_vary;

                var top_axial_force = ult_axial * (area_top) / (area_top + area_bottom);

                for (int i = 1; i <= 99; i++)
                {
                    momarray[i, 43] = momarray[i, 1] / ((momarray[i, 13] + 0.5 * (top_flg_thick + bottom_flg_thick)) / 1000);
                }

                var max_top_force = double.MinValue;

                for (int i = 1; i <= 99; i++)
                { 
                    if(momarray[i, 43] > max_top_force)
                    {
                        max_top_force = momarray[i, 43];
                    }
                }

                var extra = 0.5 * (top_flg_thick + bottom_flg_thick);

                var nett_bm = bmoment(order, 0);

                var force_at_zero = nett_bm * 1000 / (depth_left + extra);

                nett_bm = bmoment(order, span);

                var force_at_span = nett_bm * 1000 / (depth_left + extra);

                max_top_force = new List<double> { max_top_force, force_at_zero, force_at_span }.Max();

                var min_top_force = double.MaxValue;

                for (int i = 1; i <= 99; i++)
                {
                    if (momarray[i, 43] < max_top_force)
                    {
                        min_top_force = momarray[i, 43];
                    }
                }

                min_top_force = new List<double> { min_top_force, force_at_zero, force_at_span }.Min();

                var nett_max_top_force = max_top_force + top_axial_force;

                var nett_min_top_force = min_top_force + top_axial_force;

                string topflange_status;
                double max_top_ute;
                double top_tens_ute = -1;

                if (nett_max_top_force <= 0 && nett_min_top_force <= 0)
                {
                    topflange_status = "tension";
                    max_top_ute = -nett_min_top_force / resi_top;
                }
                else if(nett_max_top_force >= 0 && nett_min_top_force >= 0)
                {
                    topflange_status = "compression";
                    max_top_ute = nett_max_top_force / resi_top;
                }
                else
                {
                    topflange_status = "both";
                    var top_comp_ute = nett_max_top_force / resi_top;
                    top_tens_ute = -nett_min_top_force / resi_top;
                    max_top_ute = Math.Max(top_comp_ute, top_tens_ute);
                }

                var captions = new List<string>();

                captions.Add("Top flange is fully restrained");
                captions.Add($"Effective area = {Math.Round(aeff_top, 0)} mm2");
                captions.Add($"Resistance = {Math.Round(resi_top, 0)} kN");

                if(topflange_status == "tension")
                {
                    captions.Add($"Force due to moment = {Math.Round(min_top_force, 0)} kN");
                    captions.Add($"Total = {Math.Round(nett_min_top_force, 0)} kN");
                }
                else if (topflange_status == "compression")
                {
                    captions.Add($"Force due to moment = {Math.Round(max_top_force, 0)} kN");
                    captions.Add($"Total =  {Math.Round(nett_max_top_force, 0)} kN");
                }
                else if(topflange_status == "both")
                {
                    if(max_top_ute == top_tens_ute)
                    {
                        captions.Add($"Force due to moment = {Math.Round(min_top_force, 0)} kN");
                        captions.Add($"Total = {Math.Round(nett_min_top_force, 0)} kN");
                    }
                    else
                    {
                        captions.Add($"Force due to moment = {Math.Round(max_top_force, 0)} kN");
                        captions.Add($"Total = {Math.Round(nett_max_top_force, 0)} kN");
                    }
                }

                captions.Add($"Force from axial = {Math.Round(top_axial_force, 0)} kN");
                captions.Add($"Utilisation = {Math.Round(max_top_ute, 2)}");

                return new
                {
                    captions
                };
            }
        }

        private List<string> GetTopFlangePositionCaptions(double[,] ltbtop, double aeff_top, double top_flange_tension_resi, double top_axial_force, int seg_no)
        {
            var captions = new List<string>();

            captions.Add($"Segment from {Math.Round(ltbtop[seg_no - 1, 1], 3)} m to {Math.Round(ltbtop[seg_no, 1], 3)} m");
            captions.Add($"Segment length = {Math.Round(ltbtop[seg_no, 2] * 1000, 0)} mm");

            if (ltbtop[seg_no, 29] == 1)
            {
                captions.Add($"Correction factor kc = {Math.Round(ltbtop[seg_no, 10], 2)}");
                captions.Add($"Slenderness = {Math.Round(ltbtop[seg_no, 12], 3)}");
                captions.Add($"Effective area = {Math.Round(aeff_top, 0)} mm2");
                captions.Add($"Resistance = {Math.Round(ltbtop[seg_no, 15], 0)} kN");
                captions.Add($"Force due to moment = {Math.Round(ltbtop[seg_no, 19], 0)}  kN");
                captions.Add($"Force from axial = {Math.Round(top_axial_force, 0)} kN");
                captions.Add($"Total = {Math.Round(ltbtop[seg_no, 26], 0)} kN");
                captions.Add($"Utilisation = {Math.Round(ltbtop[seg_no, 23], 2)}");
            }
            else if (ltbtop[seg_no, 29] == -1)
            {
                captions.Add($"Force due to moment = {Math.Round(ltbtop[seg_no, 27], 0)} kN");
                captions.Add($"Force from axial = {Math.Round(top_axial_force, 0)} kN");
                captions.Add($"Total = {Math.Round(ltbtop[seg_no, 21], 0)} kN");
                captions.Add("Segment is in tension");
                captions.Add($"Flange tension resistance = {Math.Round(top_flange_tension_resi, 0)} kN");
            }
            else
            {
                captions.Add("Segment is partially in tension and part in compression");

                if(ltbtop[seg_no, 30] == ltbtop[seg_no, 22])
                {
                    captions.Add($"Force due to moment = {Math.Round(ltbtop[seg_no, 27], 0)} kN");
                    captions.Add($"Total = {Math.Round(ltbtop[seg_no, 21], 0)} kN");
                }
                else
                {
                    captions.Add($"Force due to moment = {Math.Round(ltbtop[seg_no, 19], 0)} kN");
                    captions.Add($"Total = {Math.Round(ltbtop[seg_no, 26], 0)} kN");
                }

                captions.Add($"Force from axial = {Math.Round(top_axial_force, 0)} kN");

                var ute_condition = ltbtop[seg_no, 30] == ltbtop[seg_no, 22] ? " tension" : " compression";

                captions.Add($"Most onerous utilisation ={Math.Round(ltbtop[seg_no, 30], 2)} considering {ute_condition}");
            }

            return captions;
        }

        private List<string> GetTopFlangeMaxPositionCaptions(string top_ute_condition, double[,] ltbtop, double aeff_top, double top_flange_tension_resi, double top_axial_force, int seg_no)
        {
            var captions = new List<string>();

            captions.Add($"Segment from {Math.Round(ltbtop[seg_no - 1, 1], 3)} m to {Math.Round(ltbtop[seg_no, 1], 3)} m");
            captions.Add($"Segment length = {Math.Round(ltbtop[seg_no, 2] * 1000, 0)} mm");

            if (ltbtop[seg_no, 29] == 1)
            {
                captions.Add($"Correction factor kc = {Math.Round(ltbtop[seg_no, 10], 2)}");
                captions.Add($"Slenderness = {Math.Round(ltbtop[seg_no, 12], 3)}");
                captions.Add($"Effective area = {Math.Round(aeff_top, 0)} mm2");
                captions.Add($"Resistance = {Math.Round(ltbtop[seg_no, 15], 0)} kN");
                captions.Add($"Force due to moment = {Math.Round(ltbtop[seg_no, 19], 0)}  kN");
                captions.Add($"Force from axial = {Math.Round(top_axial_force, 0)} kN");
                captions.Add($"Total = {Math.Round(ltbtop[seg_no, 26], 0)} kN");
                captions.Add($"Utilisation = {Math.Round(ltbtop[seg_no, 23], 2)}");
            }
            else if (ltbtop[seg_no, 29] == -1)
            {
                captions.Add($"Force due to moment = {Math.Round(ltbtop[seg_no, 27], 0)} kN");
                captions.Add($"Force from axial = {Math.Round(top_axial_force, 0)} kN");
                captions.Add($"Total = {Math.Round(ltbtop[seg_no, 21], 0)} kN");
                captions.Add("Segment is in tension");
                captions.Add($"Flange tension resistance = {Math.Round(top_flange_tension_resi, 0)} kN");
            }
            else
            {
                captions.Add("Segment is partially in tension and part in compression");
                captions.Add($"Most onerous utilisation ={Math.Round(ltbtop[seg_no, 30], 2)} considering {top_ute_condition}");
            }

            return captions;
        }

        private double designstrength(Order order, int thickness, int steelgrade)
        {
            var localization = order.Localization;
            double thicknessOutput = 0;

            if (steelgrade == 275) 
            {
                if (thickness <= 16)
                {
                    thicknessOutput = localization.DesignParameters.SteelGradeS235LessThan16mm;
                }
                else if (thickness > 16 && thickness <= 40)
                {
                    thicknessOutput = localization.DesignParameters.SteelGradeS235Between16and40mm;
                }
                else if(thickness > 40 && thickness <= 63)
                {
                    thicknessOutput = localization.DesignParameters.SteelGradeS235Between40and63mm;
                }
            }
            else if((steelgrade == 355))
            {
                if (thickness <= 16)
                {
                    thicknessOutput = localization.DesignParameters.SteelGradeS355LessThan16mm;
                }
                else if (thickness > 16 && thickness <= 40)
                {
                    thicknessOutput = localization.DesignParameters.SteelGradeS355Between16and40mm;
                }
                else if (thickness > 40 && thickness <= 63)
                {
                    thicknessOutput = localization.DesignParameters.SteelGradeS355Between40and63mm;
                }
            }

            return thicknessOutput;
        }

        private double bmoment(Order order, double distance)
        {
            var localization = order.Localization;
            var loading = order.Loading;

            var gamma_g = localization.DesignParameters.GammaG * localization.DesignParameters.ReductionFactorF;
            var gamma_q = localization.DesignParameters.GammaQ;

            var gamma_g_610a = localization.DesignParameters.GammaG;

            var gamma_q_610a = localization.DesignParameters.GammaQ * localization.PsiValue;

            var arrayPoints = GetArrayPoints(localization, order.Loading, gamma_g, gamma_q, gamma_g_610a, gamma_q_610a);

            var helpData = GetHelpData(order);

            var BM_reaction = helpData.lh_reaction * distance + helpData.uls_left_mom;

            var BM_udl = -helpData.uls_udl * Math.Pow(distance, 2) * 0.5;

            double BM_part = 0;

            if (distance <= helpData.part_udl_start)
            {
                BM_part = 0;
            }

            if(helpData.part_udl_start < distance && distance <= helpData.part_udl_end)
            {
                BM_part = -helpData.part_uls_udl * Math.Pow((distance - helpData.part_udl_start), 2) * 0.5;
            }

            if(helpData.part_udl_end < distance)
            {
                BM_part = -helpData.part_uls_udl * (helpData.part_udl_end - helpData.part_udl_start) *
                    (distance - 0.5 * (helpData.part_udl_start + helpData.part_udl_end));
            }

            double BM_points = 0;

            for (int i = 0; i < loading.PointLoads.Count; i++) 
            {
                var lever = distance - arrayPoints[i, 1];

                if(lever > 0)
                {
                    BM_points = BM_points - lever * arrayPoints[i, 4];
                }
            }

            double nett_bm = BM_reaction + BM_udl + BM_points + BM_part;

            return nett_bm;
        }

        [HttpGet("order/{id}/bottom-flange-verification")]
        public object BottomFlangeVerification(int id)
        {
            var order = _dbContext.Orders.Where(e => e.Id == id)
                .Include(e => e.BeamInfo)
                .Include(e => e.Restraint)
                .Include(e => e.Loading)
                .ThenInclude(e => e.PointLoads)
                .Include(e => e.Localization)
                .FirstOrDefault();

            if (order is null)
            {
                return NotFound();
            }

            var max_onerous = string.Empty;
            var beam = order.BeamInfo;
            var localization = order.Localization;
            var loading = order.Loading;

            const int segments = 100;
            var interval = beam.Span / segments;

            var depth_left = beam.WebDepth;

            var depth_right = beam.IsUniformDepth ? depth_left : beam.WebDepthRight;

            double leftheight = Math.Pow(Math.Pow(beam.WebDepth, 2), 0.5);
            double rightheight = Math.Pow(Math.Pow(beam.WebDepthRight, 2), 0.5);

            var gamma_g = localization.DesignParameters.GammaG * localization.DesignParameters.ReductionFactorF;
            var gamma_q = localization.DesignParameters.GammaQ;

            var momarray = new double[100, 100];

            for (int j = 1; j < segments - 1; j++)
            {
                var section_position = j * interval;
                var section_depth = leftheight - (leftheight - rightheight) * section_position / beam.Span;
                momarray[j, 13] = section_depth;
            }

            for (int j = 1; j < segments - 1; j++)
            {
                momarray[j, 25] = momarray[j, 13] + 0.5 * (beam.TopFlangeThickness + beam.BottomFlangeThickness);
            }

            var restraint = order.Restraint;

            var ltbbottom = new double[100, 50];
            var ltbtop = new double[100, 50];

            if (restraint.FullRestraintBottomFlange == false)
            {
                for (int i = 1; i <= restraint.BottomFlangeRestraints.Count; i++)
                {
                    ltbbottom[i, 1] = restraint.BottomFlangeRestraints[i - 1];
                }

                ltbbottom[1, 2] = 0;

                for (int i = 1; i <= restraint.BottomFlangeRestraints.Count; i++)
                {
                    ltbbottom[i + 1, 2] = ltbbottom[i + 1, 1] - ltbbottom[i, 1];
                }

                for (int i = 1; i <= restraint.BottomFlangeRestraints.Count; i++)
                {
                    var distance = ltbbottom[i, 1];

                    var nett_bm = bmoment(order, distance);

                    ltbbottom[i, 3] = nett_bm;
                }

                for (int i = 1; i <= restraint.BottomFlangeRestraints.Count; i++)
                {
                    for (int k = 1; k < 3; k++)
                    {
                        var distance = ltbbottom[i, 1] + k * ltbbottom[i + 1, 2] * 0.25;
                        var nett_bm = bmoment(order, distance);
                        ltbbottom[i + 1, k + 4] = nett_bm;
                    }
                }

                for (int i = 1; i <= restraint.BottomFlangeRestraints.Count; i++)
                {
                    double momstart = ltbbottom[i, 3];
                    double mom_25 = ltbbottom[i + 1, 5];
                    double mom_5 = ltbbottom[i + 1, 6];
                    double mom_75 = ltbbottom[i + 1, 7];
                    double momend = ltbbottom[i + 1, 3];

                    double segment_max = new List<double> { momstart, mom_25, mom_5, mom_75, momend }.Max();
                    double segment_min = new List<double> { momstart, mom_25, mom_5, mom_75, momend }.Max();

                    if (Math.Pow(segment_max, 2) < Math.Pow(segment_min, 2))
                    {
                        momstart = -1 * momstart;
                        mom_25 = -1 * mom_25;
                        mom_5 = -1 * mom_5;
                        mom_75 = -1 * mom_75;
                        momend = -1 * momend;
                        ltbtop[i + 1, 8] = segment_min;
                        segment_max = -1 * segment_min;
                    }
                    else
                    {
                        ltbtop[i + 1, 8] = segment_max;
                    }

                    var c_one = Math.Pow((35 * Math.Pow(segment_max, 2) / (Math.Pow(segment_max, 2) + 9 * Math.Pow(mom_25, 2) + 16 * Math.Pow(mom_5, 2) + 9 * Math.Pow(mom_75, 2))), 0.5);

                    ltbbottom[i + 1, 9] = c_one;
                    ltbbottom[i + 1, 10] = Math.Pow(c_one, -0.5);
                }

                var steelbottom = beam.BottomFlangeSteel == SteelType.S355 ? 355 : 275;

                var top_flg_thick = beam.TopFlangeThickness;
                var top_flg_width = beam.TopFlangeWidth;
                var bottom_flg_thick = beam.BottomFlangeThickness;
                var bottom_flg_width = beam.BottomFlangeWidth;

                var top_area = top_flg_thick * top_flg_width;
                var bottom_area = bottom_flg_thick * bottom_flg_width;

                var bottomstrength = designstrength(order, bottom_flg_thick, steelbottom);

                var epsilon_bottom = Math.Pow((235 / bottomstrength), 0.5);
                var lambda_one_bottom = 93.9 * epsilon_bottom;

                var span = beam.Span;
                double beta = 0;

                if (span * 1000 / (bottom_flg_width * 0.5) > 50)
                {
                    beta = 1;
                }
                else
                {
                    var kappa = 0.5 * bottom_flg_width / (span * 1000);

                    if (kappa <= 0.02)
                    {
                        beta = 1;
                    }
                    else if (0.02 < kappa && kappa <= 0.7)
                    {
                        beta = 1 / (1 + 6.4 * Math.Pow(kappa, 2));
                    }
                    else if (kappa > 0.7)
                    {
                        beta = 1 / (5.9 * kappa);
                    }
                }

                var psi_bottom = 1.25 * (beta - 0.2);

                var k_sigma = psi_bottom == 1 ? 0.43 : 0.578 / (psi_bottom + 0.34);

                var b_bar_bottom = 0.5 * (bottom_flg_width - 0.5 * 40);

                var lambda_p_bottom = (b_bar_bottom / top_flg_thick) / (28.4 * epsilon_bottom * Math.Pow(k_sigma, 0.5));

                var rho_bottom = lambda_p_bottom <= 0.748 ? 1 : Math.Min(1, (lambda_p_bottom - 0.188) / Math.Pow(lambda_p_bottom, 2));

                var aeff_bottom = rho_bottom * bottom_flg_thick * bottom_flg_width;
                var aeff_bottom_tension = beta * bottom_flg_thick * bottom_flg_width;

                var bottom_flange_tension_resi = aeff_bottom_tension * bottomstrength / 1000;

                var gyra_bottom = Math.Pow(top_flg_thick * Math.Pow(top_flg_width, 3) / 12 / (top_flg_thick * top_flg_width), 0.5);

                var alpha = top_flg_thick <= 40 ? 0.49 : 0.76;

                for (int i = 2; i <= restraint.BottomFlangeRestraints.Count; i++)
                {
                    ltbbottom[i, 11] = (ltbbottom[i, 1] - ltbbottom[i - 1, 1]) * 1000;
                    ltbbottom[i, 12] = ltbbottom[i, 10] * ltbbottom[i, 11] / (gyra_bottom * lambda_one_bottom);
                    ltbbottom[i, 13] = 0.5 * (1 + alpha * (ltbbottom[i, 12] - 0.2) + Math.Pow(ltbbottom[i, 12], 2));
                    ltbbottom[i, 14] = Math.Min(1, 1 / (ltbbottom[i, 13] + Math.Pow((Math.Pow(ltbbottom[i, 13], 2) - Math.Pow(ltbbottom[i, 12], 2)), 0.5)));
                    ltbbottom[i, 15] = aeff_bottom * ltbbottom[i, 14] * bottomstrength / 1000;
                }

                for (int i = 1; i <= restraint.BottomFlangeRestraints.Count; i++)
                {
                    ltbbottom[i, 16] = depth_left - (depth_left - depth_right) * (ltbbottom[i, 1] / span);
                }

                var extra = 0.5 * (top_flg_thick + bottom_flg_thick);

                for (int i = 2; i <= restraint.BottomFlangeRestraints.Count; i++)
                {
                    ltbbottom[i, 17] = ltbbottom[i - 1, 3] / ((ltbbottom[i - 1, 16] + 0.5 * (top_flg_thick + bottom_flg_thick)) / 1000);
                    ltbbottom[i, 18] = ltbbottom[i, 3] / ((ltbbottom[i, 16] + 0.5 * (top_flg_thick + bottom_flg_thick)) / 1000);

                    var max_segment_force = double.MinValue;

                    for (int g = 1; g < segments - 1; g++)
                    {
                        if ((g * interval) >= ltbbottom[i - 1, 1] && g * interval <= ltbbottom[i, 1])
                        {
                            double force_at_g = momarray[g, 1] / ((momarray[g, 13] + extra) / 1000);

                            if (force_at_g > max_segment_force)
                            {
                                max_segment_force = force_at_g;
                            }
                        }
                    }

                    var min_segment_force = double.MaxValue;

                    for (int g = 1; g < segments - 1; g++)
                    {
                        if ((g * interval) >= ltbbottom[i - 1, 1] && g * interval <= ltbbottom[i, 1])
                        {
                            double force_at_g = momarray[g, 1] / ((momarray[g, 13] + extra) / 1000);

                            if (force_at_g < min_segment_force)
                            {
                                min_segment_force = force_at_g;
                            }
                        }
                    }

                    ltbbottom[i, 25] = min_segment_force;
                    ltbbottom[i, 24] = new List<double> { ltbbottom[i, 25], ltbbottom[i, 17], ltbbottom[i, 18] }.Min();
                    ltbbottom[i, 20] = max_segment_force;
                    ltbbottom[i, 19] = new List<double> { ltbbottom[i, 17], ltbbottom[i, 18], ltbbottom[i, 20] }.Max();

                    if(ltbbottom[i, 19] < 0.00001)
                    {
                        ltbbottom[i, 19] = 0;
                    }

                    if(ltbbottom[i, 24] <= 0 && ltbbottom[i, 19] <= 0)
                    {
                        ltbbottom[i, 28] = -1;
                    }
                    else if(ltbbottom[i, 24] >= 0 && ltbbottom[i, 19] >= 0)
                    {
                        ltbbottom[i, 28] = 1;
                    }
                    else
                    {
                        ltbbottom[i, 28] = 99;
                    }
                }

                var axial_perm = loading.PermanentLoads.AxialForce;
                var axial_vary = loading.VariableLoads.AxialForce;

                var ult_axial = gamma_g * axial_perm + gamma_q * axial_vary;
                var bottom_axial_force = ult_axial * bottom_area / (bottom_area + top_area);

                for (int i = 2; i <= restraint.BottomFlangeRestraints.Count; i++)
                {
                    ltbbottom[i, 21] = ltbbottom[i, 19] + bottom_axial_force;
                    ltbbottom[i, 26] = ltbbottom[i, 24] + bottom_axial_force;

                    if (ltbbottom[i, 21] <= 0 && ltbbottom[i, 26] <= 0)
                    {
                        ltbbottom[i, 29] = -1;
                    }
                    else if (ltbbottom[i, 21] >= 0 && ltbbottom[i, 26] >= 0)
                    {
                        ltbbottom[i, 29] = 1;
                    }
                    else
                    {
                        ltbbottom[i, 29] = 99;
                    }

                    if (ltbbottom[i, 29] == -1)
                    {
                        ltbbottom[i, 22] = -ltbbottom[i, 26] / bottom_flange_tension_resi;
                    }
                    else if (ltbbottom[i, 29] == 1)
                    {
                        ltbbottom[i, 23] = ltbbottom[i, 21] / ltbtop[i, 15];
                    }
                    else
                    {
                        ltbbottom[i, 22] = -ltbbottom[i, 26] / bottom_flange_tension_resi;
                        ltbbottom[i, 23] = ltbbottom[i, 21] / ltbbottom[i, 15];

                        ltbbottom[i, 30] = Math.Max(ltbbottom[i, 22], ltbbottom[i, 23]);

                        max_onerous = ltbbottom[i, 30] == ltbbottom[i, 22] ? "tension" : "compression";
                    }
                }

                double max_bottom_ute = 0;
                var max_position = 0;

                for (int i = 2; i <= restraint.BottomFlangeRestraints.Count; i++)
                {
                    var bottom_ute = Math.Max(ltbbottom[i, 22], ltbbottom[i, 23]);

                    if (bottom_ute > max_bottom_ute)
                    {
                        max_bottom_ute = bottom_ute;
                        max_position = i;
                    }
                }


                var verificationItems = new List<VerificationItem>();

                for (int i = 2; i <= restraint.BottomFlangeRestraints.Count; i++)
                {
                    var verificationItem = new VerificationItem
                    {
                        From = Math.Round(ltbbottom[i - 1, 1], 3),
                        To = Math.Round(ltbbottom[i, 1], 3)
                    };

                    if (ltbbottom[i, 29] == -1)
                    {
                        verificationItem.DesignForce = Math.Round(ltbbottom[i, 26], 0);
                        verificationItem.Resistance = Math.Round(bottom_flange_tension_resi, 0);
                        verificationItem.Utilization = Math.Round(ltbbottom[i, 22], 2);
                    }
                    else if (ltbbottom[i, 29] == 1)
                    {
                        verificationItem.DesignForce = Math.Round(ltbtop[i, 21], 0);
                        verificationItem.Resistance = Math.Round(ltbtop[i, 15], 0);
                        verificationItem.Utilization = Math.Round(ltbtop[i, 23], 2);
                    }
                    else if (ltbbottom[i, 29] == 99)
                    {
                        if (max_onerous == "tension")
                        {
                            verificationItem.DesignForce = Math.Round(ltbtop[i, 26], 0);
                            verificationItem.Resistance = Math.Round(bottom_flange_tension_resi, 0);
                        }
                        else if(max_onerous == "compression")
                        {
                            verificationItem.DesignForce = Math.Round(ltbtop[i, 21], 0);
                            verificationItem.Resistance = Math.Round(ltbtop[i, 15], 0);
                        }

                        verificationItem.Utilization = Math.Round(ltbtop[i, 30], 2);
                    }

                    verificationItem.Captions = GetBottomFlangeCaptions(ltbbottom, aeff_bottom, bottom_flange_tension_resi, bottom_axial_force, i);

                    verificationItems.Add(verificationItem);
                }

                var captions = max_position > 0 ?
                    GetBottomFlangeMaxPositionCaptions(max_onerous, ltbbottom, aeff_bottom, bottom_flange_tension_resi, bottom_axial_force, max_position) :
                    new List<string>();

                return new
                {
                    MaximumUtilization = Math.Round(max_bottom_ute, 2),
                    verificationItems,
                    captions
                };
            }
            else
            {
                var steelbot = beam.BottomFlangeSteel == SteelType.S355 ? 355 : 275;

                var bottom_flg_thick = beam.BottomFlangeThickness;
                var bottom_flg_width = beam.BottomFlangeWidth;
                var area_bottom = bottom_flg_thick * bottom_flg_width;

                var top_flg_thick = beam.TopFlangeThickness;
                var top_flg_width = beam.TopFlangeWidth;
                var area_top = top_flg_thick * top_flg_width;

                var botstrength = designstrength(order, top_flg_thick, steelbot);

                var span = beam.Span;
                double beta = 0;

                if (span * 1000 / (bottom_flg_thick * 0.5) > 50)
                {
                    beta = 1;
                }
                else
                {
                    var kappa = 0.5 * bottom_flg_thick / (span * 1000);

                    if (kappa <= 0.02)
                    {
                        beta = 1;
                    }
                    else if (0.02 < kappa && kappa <= 0.7)
                    {
                        beta = 1 / (1 + 6.4 * Math.Pow(kappa, 2));
                    }
                    else if (kappa > 0.7)
                    {
                        beta = 1 / (5.9 * kappa);
                    }
                }

                var aeff_bottom = beta * top_flg_thick * top_flg_width;

                var resi_bottom = aeff_bottom * botstrength / 1000;

                if (loading.PermanentLoads is null)
                {
                    loading.PermanentLoads = new LoadParameters();
                }

                if (loading.VariableLoads is null)
                {
                    loading.VariableLoads = new LoadParameters();
                }

                var axial_perm = loading.PermanentLoads.AxialForce;
                var axial_vary = loading.VariableLoads.AxialForce;

                var ult_axial = gamma_g * axial_perm + gamma_q * axial_vary;

                var bottom_axial_force = ult_axial * (area_bottom) / (area_top + area_bottom);

                for (int i = 1; i <= 99; i++)
                {
                    momarray[i, 45] = momarray[i, 1] / ((momarray[i, 13] + 0.5 * (top_flg_thick + bottom_flg_thick)) / 1000);
                }

                var max_bottom_force = double.MinValue;

                for (int i = 1; i <= 99; i++)
                {
                    if (momarray[i, 45] > max_bottom_force)
                    {
                        max_bottom_force = momarray[i, 43];
                    }
                }

                var extra = 0.5 * (top_flg_thick + bottom_flg_thick);

                var nett_bm = bmoment(order, 0);

                var force_at_zero = nett_bm * 1000 / (depth_left + extra);

                nett_bm = bmoment(order, span);

                var force_at_span = nett_bm * 1000 / (depth_left + extra);

                max_bottom_force = new List<double> { max_bottom_force, force_at_zero, force_at_span }.Max();

                var min_bottom_force = double.MaxValue;

                for (int i = 1; i <= 99; i++)
                {
                    if (momarray[i, 43] < min_bottom_force)
                    {
                        min_bottom_force = momarray[i, 43];
                    }
                }

                min_bottom_force = new List<double> { min_bottom_force, force_at_zero, force_at_span }.Min();

                var nett_max_bottom_force = max_bottom_force + bottom_axial_force;

                var nett_min_bottom_force = min_bottom_force + bottom_axial_force;

                string flange_status;
                double max_bottom_ute;
                double bottom_tens_ute = -1;

                if (nett_max_bottom_force <= 0 && nett_min_bottom_force <= 0)
                {
                    flange_status = "tension";
                    max_bottom_ute = -nett_min_bottom_force / resi_bottom;
                }
                else if (nett_max_bottom_force >= 0 && nett_min_bottom_force >= 0)
                {
                    flange_status = "compression";
                    max_bottom_ute = nett_max_bottom_force / resi_bottom;
                }
                else
                {
                    flange_status = "both";
                    var top_comp_ute = nett_max_bottom_force / resi_bottom;
                    bottom_tens_ute = -nett_min_bottom_force / resi_bottom;
                    max_bottom_ute = Math.Max(top_comp_ute, bottom_tens_ute);
                }

                var captions = new List<string>();

                captions.Add("Bottom flange is fully restrained");
                captions.Add($"Effective area = {Math.Round(aeff_bottom, 0)} mm2");
                captions.Add($"Resistance = {Math.Round(resi_bottom, 0)} kN");

                if (flange_status == "tension")
                {
                    captions.Add($"Force due to moment = {Math.Round(min_bottom_force, 0)} kN");
                    captions.Add($"Total = {Math.Round(nett_min_bottom_force, 0)} kN");
                }
                else if (flange_status == "compression")
                {
                    captions.Add($"Force due to moment = {Math.Round(max_bottom_force, 0)} kN");
                    captions.Add($"Total =  {Math.Round(nett_max_bottom_force, 0)} kN");
                }
                else if (flange_status == "both")
                {
                    if (max_bottom_ute == bottom_tens_ute)
                    {
                        captions.Add($"Force due to moment = {Math.Round(min_bottom_force, 0)} kN");
                        captions.Add($"Total = {Math.Round(nett_min_bottom_force, 0)} kN");
                    }
                    else
                    {
                        captions.Add($"Force due to moment = {Math.Round(max_bottom_force, 0)} kN");
                        captions.Add($"Total = {Math.Round(nett_max_bottom_force, 0)} kN");
                    }
                }

                captions.Add($"Force from axial = {Math.Round(bottom_axial_force, 0)} kN");
                captions.Add($"Utilisation = {Math.Round(max_bottom_ute, 2)}");

                return new
                {
                    captions
                };
            }
        }

        [HttpGet("order/{id}/web-verification")]
        public object WebVerification(int id)
        {
            var order = _dbContext.Orders.Where(e => e.Id == id)
                .Include(e => e.BeamInfo)
                .Include(e => e.Restraint)
                .Include(e => e.Loading)
                .ThenInclude(e => e.PointLoads)
                .Include(e => e.Localization)
                .FirstOrDefault();

            if (order is null)
            {
                return NotFound();
            }

            var beam = order.BeamInfo;
            var loading = order.Loading;

            double webyield = beam.WebSteel == SteelType.S275 ? 275 : 355;

            webyield = 0.9 * webyield;

            var web = beam.WebThickness;

            var fourthroot = web switch
            {
                1.5 => 10900000,
                2 => 16780000,
                2.5 => 23470000,
                3 => 30880000
            };

            var span = beam.Span;

            var interval = span / 100;
            var segments = 100;

            var localization = order.Localization;

            var depth_left = beam.WebDepth;

            var depth_right = beam.IsUniformDepth ? depth_left : beam.WebDepthRight;

            double leftheight = Math.Pow(Math.Pow(beam.WebDepth, 2), 0.5);
            double rightheight = Math.Pow(Math.Pow(beam.WebDepthRight, 2), 0.5);

            var gamma_g = localization.DesignParameters.GammaG * localization.DesignParameters.ReductionFactorF;
            var gamma_q = localization.DesignParameters.GammaQ;
            var gamma_g_610a = localization.DesignParameters.GammaG;
            var gamma_q_610a = localization.DesignParameters.GammaQ * localization.PsiValue;
            
            var momarray = new double[100, 100];

            var helpData = GetHelpData(order);

            var lh_reaction = helpData.lh_reaction;
            var rh_reaction = helpData.rh_reaction;
            var uls_udl = helpData.uls_udl;
            var sls_udl = helpData.sls_udl;
            var unfactored_uls = helpData.unfactored_uls;
            var part_udl_start = helpData.part_udl_start;
            var part_udl_end = helpData.part_udl_end;

            var part_uls_udl = helpData.part_uls_udl;
            var part_sls_udl = helpData.part_sls_udl;
            var part_unfactored_udl = helpData.part_unfactored_udl;

            double[,] arraypoints = GetArrayPoints(localization, loading, gamma_g, gamma_q, gamma_g_610a, gamma_q_610a);

            for (int j = 1; j < segments; j++)
            {
                double shear_udl = -uls_udl * (j * interval);

                double shear_points = 0;

                for (int z = 0; z < loading.PointLoads.Count; z++)
                {
                    var lever = (j * interval) - arraypoints[z, 1];

                    if (lever > 0)
                    {
                        shear_points = shear_points - arraypoints[z, 4];
                    }
                }

                double shear_part = 0;

                if ((j * interval) <= part_udl_start)
                {
                    shear_part = 0;
                }

                if (part_udl_start < (j * interval) && (j * interval) <= part_udl_end)
                {
                    shear_part = -part_uls_udl * ((j * interval) - part_udl_start);
                }

                if (part_udl_end < (j * interval))
                {
                    shear_part = -part_uls_udl * (part_udl_end - part_udl_start);
                }

                double nett_shear = lh_reaction + shear_udl + shear_points + shear_part;

                momarray[j, 2] = nett_shear;
            }

            for (int j = 1; j < segments - 1; j++)
            {
                var section_position = j * interval;
                var section_depth = leftheight - (leftheight - rightheight) * section_position / span;
                momarray[j, 13] = section_depth;
            }

            for (int j = 1; j < segments - 1; j++)
            {
                momarray[j, 30] = 32.4 * fourthroot / (web * Math.Pow(momarray[j, 13], 2));
            }

            for (int j = 1; j < segments - 1; j++)
            {
                momarray[j, 31] = Math.Pow((webyield / (momarray[j, 30] * Math.Pow(3, 0.5))), 0.5);
                momarray[j, 32] = Math.Min(1, 1.5 / (0.5 + Math.Pow(momarray[j, 31], 2)));
                momarray[j, 33] = momarray[j, 32] * web * webyield * momarray[j, 13] / (Math.Pow(3, 0.5) * 1000);
            }

            var weblocalbuckle = true;

            for (int j = 1; j < segments - 1; j++)
            {
                momarray[j, 35] = (5.34 + (40 * 178) / (momarray[j, 13] * web)) * (189800) * Math.Pow((web / 178), 2);
                momarray[j, 36] = Math.Pow(webyield / (momarray[j, 35] * Math.Pow(3, 0.5)), 0.5);
                momarray[j, 37] = Math.Min(1, 1.15 / (0.9 + momarray[j, 36]));
                momarray[j, 38] = momarray[j, 37] * web * webyield * momarray[j, 13] / (Math.Pow(3, 0.5) * 1000);

                momarray[j, 39] = weblocalbuckle ? Math.Min(momarray[j, 38], momarray[j, 33]) : momarray[j, 33];

                momarray[j, 40] = Math.Pow(Math.Pow(momarray[j, 2] / momarray[j, 39], 2), 0.5);
            }

            double abs_max_shear;
            string max_shear_position;
            double critical_depth;

            if (lh_reaction > -rh_reaction)
            {
                abs_max_shear = lh_reaction;
                max_shear_position = "left end";
                critical_depth = leftheight;
            }
            else
            {
                abs_max_shear = -rh_reaction;
                max_shear_position = "right end";
                critical_depth = rightheight;
            }

            var crit_stress = 32.4 * fourthroot / (web * Math.Pow(critical_depth, 2));
            var crit_slender = Math.Pow(webyield / (crit_stress * Math.Pow(3, 0.5)), 0.5);
            var crit_reduction = Math.Min(1, 1.5 / (0.5 + Math.Pow(crit_slender, 2)));
            var crit_global_resi = crit_reduction * web * webyield * critical_depth / (Math.Pow(3, 0.5) * 1000);
            var crit_global_ute = abs_max_shear / crit_global_resi;

            double crit_local_stress = Math.Pow((5.34 + (40 * 178) / (critical_depth * web)) * (189800) * (web / 178), 2);
            var crit_local_slenderness = Math.Pow(webyield / (crit_local_stress * Math.Pow(3, 0.5)), 0.5);
            var crit_local_reduction = Math.Min(1, 1.15 / (0.9 + crit_local_slenderness));
            var crit_local_resi = crit_local_reduction * web * webyield * critical_depth / (Math.Pow(3, 0.5) * 1000);
            var crit_local_ute = abs_max_shear / crit_local_resi;

            double max_shear_ute = 0;
            var global_slenderness = crit_slender;
            var global_reduction = crit_reduction;
            var global_resi = crit_global_resi;
            var global_ute = crit_global_ute;
            var local_slenderness = crit_local_slenderness;
            var local_reduction = crit_local_reduction;
            var local_resi = crit_local_resi;
            var local_ute = crit_local_ute;

            var one = new List<Point>();
            var two = new List<Point>();
            var three = new List<Point>();
            var four = new List<Point>();
            var five = new List<Point>();

            for (int i = 0; i < segments - 1 ; i++)
            {
                var pointX = i * interval;

                one.Add(new Point(pointX, Math.Round(momarray[i, 2], 3)));
                two.Add(new Point(pointX, Math.Round(momarray[i, 33], 3)));
                three.Add(new Point(pointX, Math.Round(momarray[i, 38], 3)));
                four.Add(new Point(pointX, Math.Round(momarray[i, 39], 3)));
                five.Add(new Point(pointX, Math.Round(momarray[i, 40], 3)));
            }

            max_shear_ute = weblocalbuckle ? Math.Max(global_ute, local_ute) : global_ute;

            var caption = $"Maximum web utilisation = {Math.Round(max_shear_ute, 2)} at {max_shear_position} with {web} mm web";

            return new
            {
                caption,
                one, 
                two, 
                three, 
                four, 
                five
            };
        }

        private List<string> GetBottomFlangeMaxPositionCaptions(string max_onerous, double[,] ltbbottom, double aeff_bottom, double bottom_flange_tension_resi, double bottom_axial_force, int max_position)
        {
            var captions = new List<string>();

            captions.Add($"Segment from {Math.Round(ltbbottom[max_position - 1, 1], 3)} m to {Math.Round(ltbbottom[max_position, 1], 3)} m");
            captions.Add($"Segment length = {Math.Round(ltbbottom[max_position, 2] * 1000, 0)} mm");

            if (ltbbottom[max_position, 29] == 1)
            {
                captions.Add($"Correction factor kc = {Math.Round(ltbbottom[max_position, 10], 2)}");
                captions.Add($"Slenderness = {Math.Round(ltbbottom[max_position, 12], 3)}");
                captions.Add($"Effective area = {Math.Round(aeff_bottom, 0)} mm2");
                captions.Add($"Resistance = {Math.Round(ltbbottom[max_position, 15], 0)} kN");
                captions.Add($"Force due to moment = {Math.Round(ltbbottom[max_position, 19], 0)}  kN");
                captions.Add($"Force from axial = {Math.Round(bottom_axial_force, 0)} kN");
                captions.Add($"Total = {Math.Round(ltbbottom[max_position, 21], 0)} kN");
                captions.Add($"Utilisation = {Math.Round(ltbbottom[max_position, 23], 2)}");
            }
            else if (ltbbottom[max_position, 29] == -1)
            {
                captions.Add($"Force due to moment = {Math.Round(ltbbottom[max_position, 24], 0)} kN");
                captions.Add($"Force from axial = {Math.Round(bottom_axial_force, 0)} kN");
                captions.Add($"Total = {Math.Round(ltbbottom[max_position, 26], 0)} kN");
                captions.Add("Segment is in tension");
                captions.Add($"Flange tension resistance = {Math.Round(bottom_flange_tension_resi, 0)} kN");
            }
            else
            {
                captions.Add("Segment is partially in tension and part in compression");
                captions.Add($"Most onerous utilisation ={Math.Round(ltbbottom[max_position, 30], 2)} considering {max_onerous}");
                captions.Add($"Force from axial = {Math.Round(bottom_axial_force, 0)} kN");
            }

            return captions;
        }

        private List<string> GetBottomFlangeCaptions(double[,] ltbbottom, double aeff_bottom, double bottom_flange_tension_resi, double bottom_axial_force, int seg_no)
        {
            var captions = new List<string>();

            captions.Add($"Segment from {Math.Round(ltbbottom[seg_no - 1, 1], 3)} m to {Math.Round(ltbbottom[seg_no, 1], 3)} m");
            captions.Add($"Segment length = {Math.Round(ltbbottom[seg_no, 2] * 1000, 0)} mm");

            if (ltbbottom[seg_no, 29] == 1)
            {
                captions.Add($"Correction factor kc = {Math.Round(ltbbottom[seg_no, 10], 2)}");
                captions.Add($"Slenderness = {Math.Round(ltbbottom[seg_no, 12], 3)}");
                captions.Add($"Effective area = {Math.Round(aeff_bottom, 0)} mm2");
                captions.Add($"Resistance = {Math.Round(ltbbottom[seg_no, 15], 0)} kN");
                captions.Add($"Force due to moment = {Math.Round(ltbbottom[seg_no, 19], 0)}  kN");
                captions.Add($"Force from axial = {Math.Round(bottom_axial_force, 0)} kN");
                captions.Add($"Total = {Math.Round(ltbbottom[seg_no, 21], 0)} kN");
                captions.Add($"Utilisation = {Math.Round(ltbbottom[seg_no, 23], 2)}");
            }
            else if (ltbbottom[seg_no, 29] == -1)
            {
                captions.Add($"Force due to moment = {Math.Round(ltbbottom[seg_no, 24], 0)} kN");
                captions.Add($"Force from axial = {Math.Round(bottom_axial_force, 0)} kN");
                captions.Add($"Total = {Math.Round(ltbbottom[seg_no, 26], 0)} kN");
                captions.Add("Segment is in tension");
                captions.Add($"Flange tension resistance = {Math.Round(bottom_flange_tension_resi, 0)} kN");
            }
            else
            {
                if (ltbbottom[seg_no, 30] == ltbbottom[seg_no, 22])
                {
                    captions.Add($"Force due to moment = {Math.Round(ltbbottom[seg_no, 24], 0)} kN");
                    captions.Add($"Total = {Math.Round(ltbbottom[seg_no, 26], 0)} kN");
                }
                else
                {
                    captions.Add($"Force due to moment = {Math.Round(ltbbottom[seg_no, 19], 0)} kN");
                    captions.Add($"Total = {Math.Round(ltbbottom[seg_no, 21], 0)} kN");
                }

                var ute_condition = ltbbottom[seg_no, 30] == ltbbottom[seg_no, 22] ? " tension" : " compression";

                captions.Add($"Most onerous utilisation ={Math.Round(ltbbottom[seg_no, 30], 2)} considering {ute_condition}");
            }

            return captions;
        }

        #endregion

        #region 

        private HelpData GetHelpData(Order order)
        {
            var loading = order.Loading;
            var localization = order.Localization;
            var beam = order.BeamInfo;

            var gamma_g = localization.DesignParameters.GammaG * localization.DesignParameters.ReductionFactorF;
            var gamma_q = localization.DesignParameters.GammaQ;

            var gamma_g_610a = localization.DesignParameters.GammaG;

            var gamma_q_610a = localization.DesignParameters.GammaQ * localization.PsiValue;

            if (loading.LoadType == LoadType.UltimateLoads)
            {
                gamma_g = 1;
                gamma_q = 0;
                loading.PermanentLoads = loading.UltimateLoads;
                loading.VariableLoads = new LoadParameters();
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
                if (uls_left_mom > 0)
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

            double uls_udl_610b = 0;
            double uls_udl_610a = 0;

            var applied_perm_udl = loading.PermanentLoads.Udl;

            var flanges_area = beam.TopFlangeThickness * beam.TopFlangeWidth +
                   beam.BottomFlangeThickness * beam.BottomFlangeWidth;

            var web_eff_thick = beam.WebThickness * 1;

            beam.WebDepthRight = 1000;

            var ave_web_depth = 0.5 * (beam.WebDepth + beam.WebDepthRight);

            var section_area = flanges_area + web_eff_thick * ave_web_depth;

            var self_wt = Math.Round((section_area / (1000000) * 7850 * 9.81) / 1000, digits: 4);

            var perm_udl = applied_perm_udl + self_wt;

            var vary_udl = loading.LoadType == LoadType.UltimateLoads ? 0 : loading.VariableLoads.Udl;

            if (perm_udl != 0 || vary_udl != 0)
            {
                uls_udl_610b = gamma_g * perm_udl + gamma_q * vary_udl;
                uls_udl_610a = gamma_g_610a * perm_udl + gamma_q_610a * vary_udl;
            }

            double uls_udl = 0;

            if (localization.ULSLoadExpression == ULSLoadExpression.Expression610a)
            {
                uls_udl = uls_udl_610b;
            }
            else
            {
                if (uls_udl_610a > 0)
                {
                    uls_udl = Math.Max(uls_udl_610a, uls_udl_610b);
                }
                else
                {
                    uls_udl = Math.Min(uls_udl_610a, uls_udl_610b);
                }
            }

            var udl_moment = uls_udl * beam.Span * beam.Span / 2;

            double points_moment = 0;
            double sls_points_moment = 0;
            double unfactored_points_moment = 0;

            double[,] arraypoints = GetArrayPoints(localization, loading, gamma_g, gamma_q, gamma_g_610a, gamma_q_610a);

            for (int i = 0; i < loading.PointLoads.Count; i++)
            {
                var mom_contrib = arraypoints[i, 4] * (beam.Span - arraypoints[i, 1]);
                var sls_mom_contrib = arraypoints[i, 3] * (beam.Span - arraypoints[i, 1]);
                var unfactored_mom_contrib = (arraypoints[i, 2] + arraypoints[i, 3]) * (beam.Span - arraypoints[i, 1]);

                points_moment = points_moment + mom_contrib;
                sls_points_moment = sls_points_moment + sls_mom_contrib;
                unfactored_points_moment = unfactored_points_moment + unfactored_mom_contrib;
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

            var part_udl_start = loading.PermanentLoads.PartialUdlStart;
            var part_udl_end = loading.PermanentLoads.PartialUdlEnd;

            var part_udl_moment = part_uls_udl * (part_udl_end - part_udl_start) * (beam.Span - 0.5 * (part_udl_start + part_udl_end));

            var lh_reaction = (udl_moment + points_moment + part_udl_moment - uls_right_mom - uls_left_mom) / beam.Span;


            double total_points_load = 0;

            for (int i = 0; i < loading.PointLoads.Count; i++)
            {
                total_points_load = total_points_load + arraypoints[i, 4];
            }

            double total_partial_udl = part_uls_udl * (part_udl_end - part_udl_start);
            double rh_reaction = lh_reaction - beam.Span * uls_udl - total_partial_udl - total_points_load;

            double sls_udl = 0;
            double unfactored_uls = 0;

            if (perm_udl != 0 || vary_udl != 0)
            {
                sls_udl = vary_udl;
                unfactored_uls = perm_udl + vary_udl;
            }

            var helpData = new HelpData
            {
                uls_left_mom = uls_left_mom,
                uls_right_mom = uls_right_mom,
                lh_reaction = lh_reaction,
                uls_udl = uls_udl,
                part_udl_start = part_udl_start,
                part_udl_end = part_udl_end,
                part_uls_udl = part_uls_udl,
                rh_reaction = rh_reaction,
                part_sls_udl = part_sls_udl,
                part_unfactored_udl = part_unfactored_udl,
                unfactored_uls = unfactored_uls,
                sls_udl = sls_udl,
            };

            return helpData;
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
    }

    public class RestraintDto
    {
        public bool FullRestraintTopFlange { get; set; }
        public List<double> TopFlangeRestraints { get; set; }

        public bool FullRestraintBottomFlange { get; set; }
        public List<double> BottomFlangeRestraints { get; set; }
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
        public List<UltimatePointLoadDto> UltimatePointLoads { get; set; }
        public List<CharacteristicPointLoadDto> CharacteristicPointLoads { get; set; }
        public double Span { get; internal set; }
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
}