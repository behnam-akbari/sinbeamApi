using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scaphoid.Core.Model;
using Scaphoid.Infrastructure.Data;
using Scaphoid.Infrastructure.Repositories;

namespace Schaphoid.Api.Controllers
{
    [Route("order/{orderId}/[controller]")]
    public class VerificationController : BaseController
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly WebSectionRepository _webSectionRepository;
        private readonly CombinationRepository _combinationRepository;

        public VerificationController(ApplicationDbContext dbContext,
            WebSectionRepository webSectionRepository,
            CombinationRepository combinationRepository)
        {
            _dbContext = dbContext;
            _webSectionRepository = webSectionRepository;
            _combinationRepository = combinationRepository;
        }

        #region Restraints

        [HttpGet("[action]")]
        public object Restraints(int orderId)
        {
            var order = _dbContext.Orders
                .Where(e => e.Id == orderId)
                .Include(e => e.Localization)
                .Include(e => e.Restraint)
                .FirstOrDefault();

            if (order is null)
            {
                return NotFound();
            }

            var localization = order.Localization;

            RestraintDto restraintDto;

            if (order.Restraint is null)
            {
                restraintDto = new RestraintDto
                {
                    TopFlangeRestraints = new List<double> { 0, order.Span },
                    BottomFlangeRestraints = new List<double> { 0, order.Span },
                    FullRestraintTopFlange = false,
                    FullRestraintBottomFlange = false,
                    DesignType = localization.DesignType,
                    Combination = order.CombinationType
                };
            }
            else
            {
                var restraint = order.Restraint;

                restraintDto = new RestraintDto
                {
                    TopFlangeRestraints = restraint.TopFlangeRestraints,
                    BottomFlangeRestraints = restraint.BottomFlangeRestraints,
                    FullRestraintTopFlange = restraint.FullRestraintTopFlange,
                    FullRestraintBottomFlange = restraint.FullRestraintBottomFlange,
                    DesignType = localization.DesignType,
                    Combination = order.CombinationType
                };
            }

            restraintDto.Links.Add(new Link("save-restraint", Url.Action(nameof(Restraints),
                null, new { orderId = orderId },
                Request.Scheme),
                HttpMethods.Post));

            restraintDto.Links.Add(new Link("set-web-local-buckle", Url.Action(nameof(SetWebLocalBuckle),
                null, new { orderId = orderId },
                Request.Scheme),
                HttpMethods.Post));

            restraintDto.Links.Add(new Link("unset-web-local-buckle", Url.Action(nameof(UnsetWebLocalBuckle),
                null, new { orderId = orderId },
                Request.Scheme),
                HttpMethods.Post));

            return restraintDto;
        }

        [HttpPost("[action]")]
        public IActionResult Restraints(int orderId, [FromBody]RestraintDto restraintDto)
        {
            var order = _dbContext.Orders
                .Where(e => e.Id == orderId)
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

        [HttpGet("top-flange")]
        public object TopFlangeVerification(int orderId)
        {
            var order = _dbContext.Orders.Where(e => e.Id == orderId)
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
            var localization = order.Localization;
            var loading = order.Loading;

            if (localization.DesignType == DesignType.Iran)
            {
                if (order.CombinationType == 0)
                {
                    order.CombinationType = CombinationType.C1;
                }

                loading = _combinationRepository.GetLoading(loading, order.CombinationType);
            }

            var span = order.Span;

            const int segments = 100;
            var interval = span / segments;

            var webSection = _webSectionRepository.Get(order.Localization.SteelType, order.SectionId);

            var depth_left = webSection.WebHeight;

            var depth_right = webSection.WebHeight;

            double leftheight = Math.Pow(Math.Pow(depth_left, 2), 0.5);
            double rightheight = Math.Pow(Math.Pow(depth_right, 2), 0.5);

            var gamma_g = localization.DesignParameters.GammaG * localization.DesignParameters.ReductionFactorF;
            var gamma_q = localization.DesignParameters.GammaQ;

            var momarray = new double[100, 100];

            for (int j = 1; j < segments - 1; j++)
            {
                var section_position = j * interval;
                var section_depth = leftheight - (leftheight - rightheight) * section_position / span;
                momarray[j, 13] = section_depth;
            }

            var restraint = order.Restraint;

            if (restraint == null)
            {
                restraint = new Restraint
                {
                    TopFlangeRestraints = new List<double>() { 0, order.Span }
                };
            }

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

                    var nett_bm = bmoment(order, distance, loading);

                    ltbtop[i, 3] = nett_bm;
                }

                for (int i = 1; i <= restraint.TopFlangeRestraints.Count; i++)
                {
                    for (int k = 1; k < 3; k++)
                    {
                        var distance = ltbtop[i, 1] + k * ltbtop[i + 1, 2] * 0.25;
                        var nett_bm = bmoment(order, distance, loading);
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

                var top_flg_thick = webSection.FlangeThickness;
                var top_flg_width = webSection.FlangeWidth;
                var bottom_flg_thick = webSection.FlangeThickness;
                var bottom_flg_width = webSection.FlangeWidth;

                var top_area = top_flg_thick * top_flg_width;
                var bottom_area = bottom_flg_thick * bottom_flg_width;

                var topstrength = designstrength(order, top_flg_thick, steeltop);

                var epsilon_top = Math.Pow((235 / topstrength), 0.5);
                var lambda_one_top = 93.9 * epsilon_top;

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

                    if (ltbtop[i, 21] <= 0 && ltbtop[i, 26] <= 0)
                    {
                        ltbtop[i, 29] = -1;
                    }
                    else if (ltbtop[i, 21] >= 0 && ltbtop[i, 26] >= 0)
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
                    else if (ltbtop[i, 29] == 1)
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

                    if (top_ute > max_top_ute)
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

                    if (ltbtop[i, 29] == -1)
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
                        if (ltbtop[i, 30] == ltbtop[i, 22])
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

                if(max_position > 0)
                {
                    var x = GetTopFlangeMaxPositionCaptions2(top_ute_condition, ltbtop, aeff_top, top_flange_tension_resi, top_axial_force, max_position);

                    return new
                    {
                        MaximumUtilization = Math.Round(max_top_ute, 2),
                        verificationItems,
                        x.Captions,
                        x.KeyValues
                    };
                }

                return new
                {
                    MaximumUtilization = Math.Round(max_top_ute, 2),
                    verificationItems
                };
            }
            else
            {
                var steeltop = order.Localization.SteelType == SteelType.S355 ? 355 : 275;

                var bottom_flg_thick = webSection.FlangeThickness;
                var bottom_flg_width = webSection.FlangeWidth;
                var area_bottom = bottom_flg_thick * bottom_flg_width;

                var top_flg_thick = webSection.FlangeThickness;
                var top_flg_width = webSection.FlangeWidth;
                var area_top = top_flg_thick * top_flg_width;

                var topstrength = designstrength(order, top_flg_thick, steeltop);

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
                    if (momarray[i, 43] > max_top_force)
                    {
                        max_top_force = momarray[i, 43];
                    }
                }

                var extra = 0.5 * (top_flg_thick + bottom_flg_thick);

                var nett_bm = bmoment(order, 0, loading);

                var force_at_zero = nett_bm * 1000 / (depth_left + extra);

                nett_bm = bmoment(order, span, loading);

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
                else if (nett_max_top_force >= 0 && nett_min_top_force >= 0)
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

                var x = new X();

                x.Captions.Add("Top flange is fully restrained");
                x.KeyValues.Add("Effective area", $"{Math.Round(aeff_top, 0)} mm2");
                x.KeyValues.Add($"Resistance",$"{Math.Round(resi_top, 0)} kN");

                if (topflange_status == "tension")
                {
                    x.KeyValues.Add($"Force due to moment", $"{Math.Round(min_top_force, 0)} kN");
                    x.KeyValues.Add($"Total", $"{Math.Round(nett_min_top_force, 0)} kN");
                }
                else if (topflange_status == "compression")
                {
                    x.KeyValues.Add($"Force due to moment", $"{Math.Round(max_top_force, 0)} kN");
                    x.KeyValues.Add($"Total", $"{Math.Round(nett_max_top_force, 0)} kN");
                }
                else if (topflange_status == "both")
                {
                    if (max_top_ute == top_tens_ute)
                    {
                        x.KeyValues.Add($"Force due to moment", $"{Math.Round(min_top_force, 0)} kN");
                        x.KeyValues.Add($"Total", $"{Math.Round(nett_min_top_force, 0)} kN");
                    }
                    else
                    {
                        x.KeyValues.Add($"Force due to moment", $"{Math.Round(max_top_force, 0)} kN");
                        x.KeyValues.Add($"Total", $"{Math.Round(nett_max_top_force, 0)} kN");
                    }
                }

                x.KeyValues.Add($"Force from axial", $"{Math.Round(top_axial_force, 0)} kN");
                x.KeyValues.Add($"Utilisation", $"{Math.Round(max_top_ute, 2)}");

                return new
                {
                    x.Captions,
                    x.KeyValues
                };
            }
        }

        [HttpGet("bottom-flange")]
        public object BottomFlangeVerification(int orderId)
        {
            var order = _dbContext.Orders.Where(e => e.Id == orderId)
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
            var localization = order.Localization;
            var loading = order.Loading;

            if (localization.DesignType == DesignType.Iran)
            {
                if (order.CombinationType == 0)
                {
                    order.CombinationType = CombinationType.C1;
                }

                loading = _combinationRepository.GetLoading(loading, order.CombinationType);
            }

            var span = order.Span;
            var webSection = _webSectionRepository.Get(order.Localization.SteelType, order.SectionId);

            const int segments = 100;
            var interval = span / segments;

            var depth_left = webSection.WebHeight;

            var depth_right = webSection.WebHeight;

            double leftheight = Math.Pow(Math.Pow(depth_left, 2), 0.5);
            double rightheight = Math.Pow(Math.Pow(depth_right, 2), 0.5);

            var gamma_g = localization.DesignParameters.GammaG * localization.DesignParameters.ReductionFactorF;
            var gamma_q = localization.DesignParameters.GammaQ;

            var momarray = new double[100, 100];

            for (int j = 1; j < segments - 1; j++)
            {
                var section_position = j * interval;
                var section_depth = leftheight - (leftheight - rightheight) * section_position / span;
                momarray[j, 13] = section_depth;
            }

            for (int j = 1; j < segments - 1; j++)
            {
                momarray[j, 25] = momarray[j, 13] + 0.5 * (webSection.FlangeThickness + webSection.FlangeThickness);
            }

            var restraint = order.Restraint;

            if (restraint == null)
            {
                restraint = new Restraint
                {
                    BottomFlangeRestraints = new List<double>() { 0, order.Span }
                };
            }

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

                    var nett_bm = bmoment(order, distance, loading);

                    ltbbottom[i, 3] = nett_bm;
                }

                for (int i = 1; i <= restraint.BottomFlangeRestraints.Count; i++)
                {
                    for (int k = 1; k < 3; k++)
                    {
                        var distance = ltbbottom[i, 1] + k * ltbbottom[i + 1, 2] * 0.25;
                        var nett_bm = bmoment(order, distance, loading);
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

                var steelbottom = order.Localization.SteelType == SteelType.S355 ? 355 : 275;

                var top_flg_thick = webSection.FlangeThickness;
                var top_flg_width = webSection.FlangeWidth;
                var bottom_flg_thick = webSection.FlangeThickness;
                var bottom_flg_width = webSection.FlangeWidth;

                var top_area = top_flg_thick * top_flg_width;
                var bottom_area = bottom_flg_thick * bottom_flg_width;

                var bottomstrength = designstrength(order, bottom_flg_thick, steelbottom);

                var epsilon_bottom = Math.Pow((235 / bottomstrength), 0.5);
                var lambda_one_bottom = 93.9 * epsilon_bottom;

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

                    if (ltbbottom[i, 19] < 0.00001)
                    {
                        ltbbottom[i, 19] = 0;
                    }

                    if (ltbbottom[i, 24] <= 0 && ltbbottom[i, 19] <= 0)
                    {
                        ltbbottom[i, 28] = -1;
                    }
                    else if (ltbbottom[i, 24] >= 0 && ltbbottom[i, 19] >= 0)
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
                        ltbbottom[i, 23] = ltbbottom[i, 21] / ltbbottom[i, 15];
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
                        else if (max_onerous == "compression")
                        {
                            verificationItem.DesignForce = Math.Round(ltbtop[i, 21], 0);
                            verificationItem.Resistance = Math.Round(ltbtop[i, 15], 0);
                        }

                        verificationItem.Utilization = Math.Round(ltbtop[i, 30], 2);
                    }

                    verificationItem.Captions = GetBottomFlangeCaptions(ltbbottom, aeff_bottom, bottom_flange_tension_resi, bottom_axial_force, i);

                    verificationItems.Add(verificationItem);
                }

                if(max_position > 0)
                {
                    var x = GetBottomFlangeMaxPositionCaptions2(max_onerous, ltbbottom, aeff_bottom, bottom_flange_tension_resi, bottom_axial_force, max_position);

                    return new
                    {
                        MaximumUtilization = Math.Round(max_bottom_ute, 2),
                        verificationItems,
                        x.Captions,
                        x.KeyValues
                    };
                }
                else
                {
                    return new
                    {
                        MaximumUtilization = Math.Round(max_bottom_ute, 2),
                        verificationItems
                    };
                }
            }
            else
            {
                var steelbot = order.Localization.SteelType == SteelType.S355 ? 355 : 275;

                var bottom_flg_thick = webSection.FlangeThickness;
                var bottom_flg_width = webSection.FlangeWidth;
                var area_bottom = bottom_flg_thick * bottom_flg_width;

                var top_flg_thick = webSection.FlangeThickness;
                var top_flg_width = webSection.FlangeWidth;
                var area_top = top_flg_thick * top_flg_width;

                var botstrength = designstrength(order, top_flg_thick, steelbot);

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

                var nett_bm = bmoment(order, 0, loading);

                var force_at_zero = nett_bm * 1000 / (depth_left + extra);

                nett_bm = bmoment(order, span, loading);

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

                var x = new X();

                x.Captions.Add("Bottom flange is fully restrained");
                x.KeyValues.Add($"Effective area", $"{Math.Round(aeff_bottom, 0)} mm2");
                x.KeyValues.Add($"Resistance", $"{Math.Round(resi_bottom, 0)} kN");

                if (flange_status == "tension")
                {
                    x.KeyValues.Add($"Force due to moment", $"{Math.Round(min_bottom_force, 0)} kN");
                    x.KeyValues.Add($"Total", $"{Math.Round(nett_min_bottom_force, 0)} kN");
                }
                else if (flange_status == "compression")
                {
                    x.KeyValues.Add($"Force due to moment", $"{Math.Round(max_bottom_force, 0)} kN");
                    x.KeyValues.Add($"Total", $"{Math.Round(nett_max_bottom_force, 0)} kN");
                }
                else if (flange_status == "both")
                {
                    if (max_bottom_ute == bottom_tens_ute)
                    {
                        x.KeyValues.Add($"Force due to moment", $"{Math.Round(min_bottom_force, 0)} kN");
                        x.KeyValues.Add($"Total", $"{Math.Round(nett_min_bottom_force, 0)} kN");
                    }
                    else
                    {
                        x.KeyValues.Add($"Force due to moment", $"{Math.Round(max_bottom_force, 0)} kN");
                        x.KeyValues.Add($"Total", $"{Math.Round(nett_max_bottom_force, 0)} kN");
                    }
                }

                x.KeyValues.Add($"Force from axial", $"{Math.Round(bottom_axial_force, 0)} kN");
                x.KeyValues.Add($"Utilisation", $"{Math.Round(max_bottom_ute, 2)}");

                return new
                {
                    x.Captions,
                    x.KeyValues
                };
            }
        }

        [HttpPost("set-web-local-buckle")]
        public IActionResult SetWebLocalBuckle(int orderId)
        {
            var order = _dbContext.Orders.Where(e => e.Id == orderId)
                .Include(e => e.BeamInfo)
                .FirstOrDefault();

            if (order is null)
            {
                return NotFound();
            }

            order.BeamInfo.WebLocalBuckle = true;

            _dbContext.SaveChanges();

            return Ok();
        }

        [HttpPost("unset-web-local-buckle")]
        public IActionResult UnsetWebLocalBuckle(int orderId)
        {
            var order = _dbContext.Orders.Where(e => e.Id == orderId)
                .Include(e => e.BeamInfo)
                .FirstOrDefault();

            if (order is null)
            {
                return NotFound();
            }

            order.BeamInfo.WebLocalBuckle = false;

            _dbContext.SaveChanges();

            return Ok();
        }

        [HttpGet("web-verification")]
        public object WebVerification(int orderId)
        {
            var order = _dbContext.Orders.Where(e => e.Id == orderId)
                .Include(e => e.Restraint)
                .Include(e => e.Loading)
                .ThenInclude(e => e.PointLoads)
                .Include(e => e.Localization)
                .FirstOrDefault();

            if (order is null)
            {
                return NotFound();
            }

            var loading = order.Loading;
            var localization = order.Localization;

            if (localization.DesignType == DesignType.Iran)
            {
                if (order.CombinationType == 0)
                {
                    order.CombinationType = CombinationType.C1;
                }

                loading = _combinationRepository.GetLoading(loading, order.CombinationType);
            }

            var webSection = _webSectionRepository.Get(order.Localization.SteelType, order.SectionId);

            var web = webSection.WebThickness;

            double webyield = order.Localization.SteelType == SteelType.S235 ? 275 : 355;

            webyield = 0.9 * webyield;

            var fourthroot = web switch
            {
                1.5 => 10900000,
                2 => 16780000,
                2.5 => 23470000,
                3 => 30880000
            };

            var span = order.Span;

            var interval = span / 100;
            var segments = 100;

            var depth_left = webSection.WebHeight;

            var depth_right =  depth_left;

            double leftheight = Math.Pow(Math.Pow(webSection.WebHeight, 2), 0.5);
            double rightheight = Math.Pow(Math.Pow(webSection.WebHeight, 2), 0.5);

            var helpData = GetHelpData2(order, webSection, loading);

            var gamma_g = helpData.gamma_g;
            var gamma_q = helpData.gamma_q;
            var gamma_g_610a = helpData.gamma_g_610a;
            var gamma_q_610a = helpData.gamma_q_610a;

            var momarray = new double[100, 100];

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

            var weblocalbuckle = true;//beam.WebLocalBuckle; // ToDo

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

            for (int i = 1; i < segments - 1; i++)
            {
                double pointX = Math.Round(i * interval, 2);

                one.Add(new Point(pointX, Math.Round(momarray[i, 2], 2)));
                two.Add(new Point(pointX, Math.Round(momarray[i, 33], 2)));
                three.Add(new Point(pointX, Math.Round(momarray[i, 38], 2)));
                four.Add(new Point(pointX, Math.Round(momarray[i, 39], 2)));
                five.Add(new Point(pointX, Math.Round(momarray[i, 40], 2)));
            }

            max_shear_ute = weblocalbuckle ? Math.Max(global_ute, local_ute) : global_ute;

            var caption = $"Maximum web utilisation = {Math.Round(max_shear_ute, 2)} at {max_shear_position} with {web} mm web";

            return new
            {
                WebLocalBuckle = true,
                caption,
                one,
                two,
                three,
                four,
                five
            };
        }

        #endregion

        #region Private Methods

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

                if (ltbtop[seg_no, 30] == ltbtop[seg_no, 22])
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

        private X GetTopFlangeMaxPositionCaptions2(string top_ute_condition, double[,] ltbtop, double aeff_top, double top_flange_tension_resi, double top_axial_force, int seg_no)
        {
            var x = new X();

            x.KeyValues.Add("Segment", $"from {Math.Round(ltbtop[seg_no - 1, 1], 3)} m to {Math.Round(ltbtop[seg_no, 1], 3)} m");
            x.KeyValues.Add("Segment length", $"{Math.Round(ltbtop[seg_no, 2] * 1000, 0)} mm");

            if (ltbtop[seg_no, 29] == 1)
            {
                x.KeyValues.Add("Correction factor kc", $"{Math.Round(ltbtop[seg_no, 10], 2)}");
                x.KeyValues.Add("Slenderness", $"{Math.Round(ltbtop[seg_no, 12], 3)}");
                x.KeyValues.Add("Effective area", $"{Math.Round(aeff_top, 0)} mm2");
                x.KeyValues.Add("Resistance", $"{Math.Round(ltbtop[seg_no, 15], 0)} kN");
                x.KeyValues.Add("Force due to moment", $"{Math.Round(ltbtop[seg_no, 19], 0)}  kN");
                x.KeyValues.Add("Force from axial", $"{Math.Round(top_axial_force, 0)} kN");
                x.KeyValues.Add("Total", $"{Math.Round(ltbtop[seg_no, 26], 0)} kN");
                x.KeyValues.Add("Utilisation", $"{Math.Round(ltbtop[seg_no, 23], 2)}");
            }
            else if (ltbtop[seg_no, 29] == -1)
            {
                x.KeyValues.Add("Force due to moment",$"{Math.Round(ltbtop[seg_no, 27], 0)} kN");
                x.KeyValues.Add("Force from axial",$"{Math.Round(top_axial_force, 0)} kN");
                x.KeyValues.Add("Total",$"{Math.Round(ltbtop[seg_no, 21], 0)} kN");
                x.Captions.Add("Segment is in tension");
                x.KeyValues.Add("Flange tension resistance",$"{Math.Round(top_flange_tension_resi, 0)} kN");
            }
            else
            {
                x.Captions.Add("Segment is partially in tension and part in compression");
                x.Captions.Add($"Most onerous utilisation ={Math.Round(ltbtop[seg_no, 30], 2)} considering {top_ute_condition}");
            }

            return x;
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

        private double designstrength(Order order, double thickness, int steelgrade)
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
                else if (thickness > 40 && thickness <= 63)
                {
                    thicknessOutput = localization.DesignParameters.SteelGradeS235Between40and63mm;
                }
            }
            else if ((steelgrade == 355))
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

        private double bmoment(Order order, double distance, Loading loading)
        {
            var webSection = _webSectionRepository.Get(order.Localization.SteelType, order.SectionId);

            var localization = order.Localization;

            var gamma_g = localization.DesignParameters.GammaG * localization.DesignParameters.ReductionFactorF;
            var gamma_q = localization.DesignParameters.GammaQ;

            var gamma_g_610a = localization.DesignParameters.GammaG;

            var gamma_q_610a = localization.DesignParameters.GammaQ * localization.PsiValue;

            var arrayPoints = GetArrayPoints(localization, loading, gamma_g, gamma_q, gamma_g_610a, gamma_q_610a);

            var helpData = GetHelpData2(order, webSection, loading);

            var BM_reaction = helpData.lh_reaction * distance + helpData.uls_left_mom;

            var BM_udl = -helpData.uls_udl * Math.Pow(distance, 2) * 0.5;

            double BM_part = 0;

            if (distance <= helpData.part_udl_start)
            {
                BM_part = 0;
            }

            if (helpData.part_udl_start < distance && distance <= helpData.part_udl_end)
            {
                BM_part = -helpData.part_uls_udl * Math.Pow((distance - helpData.part_udl_start), 2) * 0.5;
            }

            if (helpData.part_udl_end < distance)
            {
                BM_part = -helpData.part_uls_udl * (helpData.part_udl_end - helpData.part_udl_start) *
                    (distance - 0.5 * (helpData.part_udl_start + helpData.part_udl_end));
            }

            double BM_points = 0;

            for (int i = 0; i < loading.PointLoads.Count; i++)
            {
                var lever = distance - arrayPoints[i, 1];

                if (lever > 0)
                {
                    BM_points = BM_points - lever * arrayPoints[i, 4];
                }
            }

            double nett_bm = BM_reaction + BM_udl + BM_points + BM_part;

            return nett_bm;
        }

        #endregion
    }


    public class RestraintDto : Resource
    {
        public bool FullRestraintTopFlange { get; set; }
        public List<double> TopFlangeRestraints { get; set; }

        public bool FullRestraintBottomFlange { get; set; }
        public List<double> BottomFlangeRestraints { get; set; }
        public DesignType DesignType { get; internal set; }
        public CombinationType Combination { get; internal set; }
    }

    public class X
    {
        public Dictionary<string, string> KeyValues { get; set; } = new Dictionary<string, string>();
        public List<string> Captions { get; set; } = new List<string>();
    }
}
