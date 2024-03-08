using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scaphoid.Core.Model;
using Scaphoid.Infrastructure.Data;
using Scaphoid.Infrastructure.Repositories;
using static System.Collections.Specialized.BitVector32;

namespace Schaphoid.Api.Controllers
{
    [ApiController]
    [Route("order/{orderId}/[controller]")]
    public class DesignController : BaseController
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly WebSectionRepository _webSectionRepository;
        private readonly FlangeRepository _flangeRepository;

        public DesignController(FlangeRepository flangeRepository, 
            WebSectionRepository webSectionRepository, 
            ApplicationDbContext dbContext)
        {
            _flangeRepository = flangeRepository;
            _webSectionRepository = webSectionRepository;
            _dbContext = dbContext;
        }

        [HttpGet]
        public object Get(int orderId)
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

            double designed_web, max_shear_ute;
            GetWebUtilization(order, out designed_web, out max_shear_ute);

            var caption = max_shear_ute > 1 ? "No satisfactory web - try increasing the beam depth" :
                    $"Thinnest web is {designed_web} mm, with a utilisation of {Math.Round(max_shear_ute, 2)}";

            var bottomFlangeUtilization = GetBottomFlangeUtilization(order);
            var topFlangeUtilization = GetTopFlangeUtilization(order);

            var x = GetBottomFlangeDesign(order);

            return new DesignDto
            {
                Web = new DesignDto.Section
                {
                    IsValid = max_shear_ute < 1,
                    Utilization = max_shear_ute
                },
                BottomFlange = new DesignDto.Section
                {
                    IsValid = bottomFlangeUtilization < 1,
                    Utilization = bottomFlangeUtilization
                },
                TopFlange = new DesignDto.Section
                {
                    IsValid = topFlangeUtilization < 1,
                    Utilization = topFlangeUtilization
                },
                Links = new List<Link>
                {
                    new Link("create-request", Url.Action(nameof(RequestController.Create),
                    "Request", new { orderId = orderId },
                    Request.Scheme),
                    HttpMethods.Post),
                    new Link("get-countries", Url.Action(nameof(CountriesController.Get),
                    "Countries", null,
                    Request.Scheme),
                    HttpMethods.Get)
                }
            };
        }

        private void GetWebUtilization(Order order, out double designed_web, out double max_shear_ute)
        {
            var webSection = _webSectionRepository.Get(order.Localization.SteelType, order.SectionId);

            max_shear_ute = GetMaxShearUte(order, webSection.WebThickness);

            max_shear_ute = Math.Round(max_shear_ute, 2);

            designed_web = webSection.WebThickness;
        }

        private void GetDesignWeb(Order order, out double designed_web, out double max_shear_ute)
        {
            designed_web = -1;
            max_shear_ute = int.MaxValue;
            foreach (var web in Constants.NewWebThicknessCollection)
            {
                max_shear_ute = GetMaxShearUte(order, web);

                if (max_shear_ute < 1)
                {
                    designed_web = web;
                    break;
                }
            }
        }

        private double GetMaxShearUte(Order order, double web)
        {
            var loading = order.Loading;

            var weblocalbuckle = true;//beam.WebLocalBuckle;//ToDo

            double webyield = order.Localization.SteelType == SteelType.S235 ? 275 : 355;

            webyield = 0.9 * webyield;

            var fourthroot = web switch
            {
                1.5 => 10900000,
                2 => 16780000,
                2.5 => 23470000,
                3 => 30880000,
                _ => throw new NotImplementedException()
            };

            var span = order.Span;

            var interval = span / 100;
            var segments = 100;

            var localization = order.Localization;

            var webSection = _webSectionRepository.Get(order.Localization.SteelType, order.SectionId);

            var depth_left = webSection.WebHeight;

            var depth_right = webSection.WebHeight;

            double leftheight = Math.Pow(Math.Pow(depth_left, 2), 0.5);
            double rightheight = Math.Pow(Math.Pow(depth_right, 2), 0.5);

            var helpData = GetHelpData(order, webSection);

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

            max_shear_ute = weblocalbuckle ? Math.Max(global_ute, local_ute) : global_ute;

            return max_shear_ute;
        }

        private double GetBottomFlangeUtilization(Order order)
        {
            var webSection = _webSectionRepository.Get(order.Localization.SteelType, order.SectionId);

            var max_bottom_ute = GetMaxBottomUte(order, webSection.FlangeThickness, webSection.FlangeWidth);

            max_bottom_ute = Math.Round(max_bottom_ute, 2);

            return max_bottom_ute;
        }

        private double GetTopFlangeUtilization(Order order)
        {
            var webSection = _webSectionRepository.Get(order.Localization.SteelType, order.SectionId);

            var max_top_ute = GetMaxTopUte(order, webSection.FlangeThickness, webSection.FlangeWidth);

            max_top_ute = Math.Round(max_top_ute, 2);

            return max_top_ute;
        }

        private FlangeDesign GetBottomFlangeDesign(Order order)
        {
            var caption = string.Empty;
            double max_bottom_ute = -1;
            int bottom_flg_thick = -1;
            double bottom_flg_width = -1;
            bool IsValid = false;

            var flanges = _flangeRepository.Get();

            foreach (var flange in flanges)
            {
                max_bottom_ute = GetMaxBottomUte(order, flange.Thickness, flange.Width);

                max_bottom_ute = Math.Round(max_bottom_ute, 2);

                if(max_bottom_ute == 1)
                {
                    max_bottom_ute = 0.99;
                }

                if (max_bottom_ute <= 1)
                {
                    bottom_flg_width = flange.Width;
                    bottom_flg_thick = flange.Thickness;
                    IsValid = true;
                    caption = $"Lightest bottom flange is {bottom_flg_width} x {bottom_flg_thick} mm, Utilisation is {max_bottom_ute}";
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(caption))
            {
                caption = "No suitable sizes - increase beam depth or steel grade";
            }

            return new FlangeDesign
            {
                Caption = caption,
                Thick = bottom_flg_thick,
                Width = bottom_flg_width,
                Utilization = max_bottom_ute,
                IsValid = IsValid
            };
        }

        private FlangeDesign GetTopFlangeDesign(Order order)
        {
            var caption = string.Empty;
            double max_top_ute = -1;
            int top_flg_thick = -1;
            double top_flg_width = -1;
            bool IsValid = false;

            var flanges = _flangeRepository.Get();

            foreach (var flange in flanges)
            {
                max_top_ute = GetMaxTopUte(order, flange.Thickness, flange.Width);

                max_top_ute = Math.Round(max_top_ute, 2);

                if(max_top_ute == 1)
                {
                    max_top_ute = 0.99;
                }

                if (max_top_ute <= 1)
                {
                    top_flg_thick = flange.Thickness;
                    top_flg_width = flange.Width;
                    IsValid = true;
                    caption = $"Lightest top flange is {top_flg_width} x {top_flg_thick} mm, Utilisation is {max_top_ute}";
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(caption))
            {
                caption = "No suitable sizes - increase beam depth or steel grade";
            }

            return new FlangeDesign
            {
                Caption = caption,
                Thick = top_flg_thick,
                Width = top_flg_width,
                Utilization = max_top_ute,
                IsValid = IsValid
            };
        }

        public double GetMaxBottomUte(Order order, double bottom_flg_thick, double bottom_flg_width)
        {
            var max_onerous = string.Empty;
            var localization = order.Localization;
            var loading = order.Loading;
            var span = order.Span;

            var webSection = _webSectionRepository.Get(order.Localization.SteelType, order.SectionId);

            const int segments = 100;
            var interval = order.Span / segments;

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
                var section_depth = leftheight - (leftheight - rightheight) * section_position / order.Span;
                momarray[j, 13] = section_depth;
            }

            for (int j = 1; j < segments - 1; j++)
            {
                momarray[j, 25] = momarray[j, 13] + 0.5 * (webSection.FlangeThickness + bottom_flg_thick);
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

                var steelbottom = order.Localization.SteelType == SteelType.S355 ? 355 : 275;

                var top_flg_thick = webSection.FlangeThickness;
                var top_flg_width = webSection.FlangeWidth;

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

                return max_bottom_ute;
            }
            else
            {
                var steelbot = order.Localization.SteelType == SteelType.S355 ? 355 : 275;

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

                double max_bottom_ute;

                if (nett_max_bottom_force <= 0 && nett_min_bottom_force <= 0)
                {
                    max_bottom_ute = -nett_min_bottom_force / resi_bottom;
                }
                else if (nett_max_bottom_force >= 0 && nett_min_bottom_force >= 0)
                {
                    max_bottom_ute = nett_max_bottom_force / resi_bottom;
                }
                else
                {
                    var top_comp_ute = nett_max_bottom_force / resi_bottom;
                    var bottom_tens_ute = -nett_min_bottom_force / resi_bottom;
                    max_bottom_ute = Math.Max(top_comp_ute, bottom_tens_ute);
                }

                return max_bottom_ute;
            }
        }

        public double GetMaxTopUte(Order order, double top_flg_thick, double top_flg_width)
        {
            var top_ute_condition = string.Empty;
            var localization = order.Localization;
            var loading = order.Loading;
            var span = order.Span;

            var webSection = _webSectionRepository.Get(order.Localization.SteelType, order.SectionId);

            double max_top_ute = 0;

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

                return max_top_ute;
            }
            else
            {
                var steeltop = order.Localization.SteelType == SteelType.S355 ? 355 : 275;

                var bottom_flg_thick = webSection.FlangeThickness;
                var bottom_flg_width = webSection.FlangeWidth;
                var area_bottom = bottom_flg_thick * bottom_flg_width;

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

                double top_tens_ute = -1;

                if (nett_max_top_force <= 0 && nett_min_top_force <= 0)
                {
                    max_top_ute = -nett_min_top_force / resi_top;
                }
                else if (nett_max_top_force >= 0 && nett_min_top_force >= 0)
                {
                    max_top_ute = nett_max_top_force / resi_top;
                }
                else
                {
                    var top_comp_ute = nett_max_top_force / resi_top;
                    top_tens_ute = -nett_min_top_force / resi_top;
                    max_top_ute = Math.Max(top_comp_ute, top_tens_ute);
                }
            }

            return max_top_ute;
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

        private double bmoment(Order order, double distance)
        {
            var webSection = _webSectionRepository.Get(order.Localization.SteelType, order.SectionId);

            var localization = order.Localization;
            var loading = order.Loading;

            var gamma_g = localization.DesignParameters.GammaG * localization.DesignParameters.ReductionFactorF;
            var gamma_q = localization.DesignParameters.GammaQ;

            var gamma_g_610a = localization.DesignParameters.GammaG;

            var gamma_q_610a = localization.DesignParameters.GammaQ * localization.PsiValue;

            var arrayPoints = GetArrayPoints(localization, order.Loading, gamma_g, gamma_q, gamma_g_610a, gamma_q_610a);

            var helpData = GetHelpData(order, webSection);

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
    }

    internal class DesignDto : Resource
    {
        public Section Web { get; set; }
        public Section BottomFlange { get; set; }
        public Section TopFlange { get; set; }

        public class Section
        {
            public bool IsValid { get; set; }
            public double Utilization { get; set; }
        }
    }
}