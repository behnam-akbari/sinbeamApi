using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scaphoid.Core.Model;
using Scaphoid.Infrastructure.Data;
using Scaphoid.Infrastructure.Repositories;

namespace Schaphoid.Api.Controllers
{
    [ApiController]
    [Route("order/{orderId}/[controller]")]
    public class AnalysisController : BaseController
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly WebSectionRepository _webSectionRepository;
        private readonly CombinationRepository _combinationRepository;

        public AnalysisController(ApplicationDbContext dbContext, 
            WebSectionRepository webSectionRepository, 
            CombinationRepository combinationRepository)
        {
            _webSectionRepository = webSectionRepository;
            _dbContext = dbContext;
            _combinationRepository = combinationRepository;
        }

        [HttpGet]
        public object Standard(int orderId)
        {
            var order = _dbContext.Orders.Where(e => e.Id == orderId)
                .Include(e => e.Localization)
                .Include(e => e.Loading)
                .ThenInclude(e => e.PointLoads)
                .FirstOrDefault();

            if (order is null)
            {
                return NotFound();
            }

            return Get(order.Span, order.SectionId, order.Localization, order.Loading);
        }

        [HttpGet("[action]")]
        public object Iran(int orderId, CombinationType combination = CombinationType.C1)
        {
            var order = _dbContext.Orders.Where(e => e.Id == orderId)
                .Include(e => e.Localization)
                .Include(e => e.Loading)
                .ThenInclude(e => e.DistributeLoads)
                .Include(e => e.Loading)
                .ThenInclude(e => e.AxialForceLoads)
                .Include(e => e.Loading)
                .ThenInclude(e => e.EndMomentLoads)
                .Include(e => e.Loading)
                .ThenInclude(e => e.XPointLoads)
                .FirstOrDefault();

            if (order is null)
            {
                return NotFound();
            }

            var loading = _combinationRepository.GetLoading(order.Loading, combination);

            return Get(order.Span, order.SectionId, order.Localization, loading);
        }

        private object Get(double span, string sectionId, Localization localization, Loading loading)
        {
            var section = _webSectionRepository.Get(localization.SteelType, sectionId);

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

            var self_wt = Math.Round((section.SectionArea / (1000000) * 7850 * 9.81) / 1000, digits: 4);

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

            var udl_moment = uls_udl * span * span / 2;
            var sls_udl_moment = sls_udl * span * span / 2;
            var unfactored_udl_moment = unfactored_uls * span * span / 2;

            var part_udl_moment = part_uls_udl * (part_udl_end - part_udl_start) * (span - 0.5 * (part_udl_start + part_udl_end));
            var part_sls_udl_moment = part_sls_udl * (part_udl_end - part_udl_start) * (span - 0.5 * (part_udl_start + part_udl_end));
            var unfactored_part_udl_moment = part_unfactored_udl * (part_udl_end - part_udl_start) * (span - 0.5 * (part_udl_start + part_udl_end));


            double points_moment = 0;
            double sls_points_moment = 0;
            double unfactored_points_moment = 0;

            for (int i = 0; i < loading.PointLoads.Count; i++)
            {
                var mom_contrib = arraypoints[i, 4] * (span - arraypoints[i, 1]);
                var sls_mom_contrib = arraypoints[i, 3] * (span - arraypoints[i, 1]);
                var unfactored_mom_contrib = (arraypoints[i, 2] + arraypoints[i, 3]) * (span - arraypoints[i, 1]);

                points_moment = points_moment + mom_contrib;
                sls_points_moment = sls_points_moment + sls_mom_contrib;
                unfactored_points_moment = unfactored_points_moment + unfactored_mom_contrib;
            }

            var lh_reaction = (udl_moment + points_moment + part_udl_moment - uls_right_mom - uls_left_mom) / span;

            ltbdata[9] = lh_reaction;

            var sls_lh_reaction = (sls_udl_moment + sls_points_moment + part_sls_udl_moment - sls_right_mom - sls_left_mom) / span;
            var unfactored_lh_reaction = (unfactored_udl_moment + unfactored_points_moment + unfactored_part_udl_moment - unfactored_left_moment - unfactored_right_moment) / span;

            var sheardef_sls_lh_reaction = (sls_udl_moment + sls_points_moment + part_sls_udl_moment) / span;
            var sheardef_unfactored_lh_reaction = (unfactored_udl_moment + unfactored_points_moment + unfactored_part_udl_moment) / span;

            const int segments = 100;
            var interval = span / segments;

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

            bmdData[segments, 0] = Math.Pow(Math.Pow(span, 2), 0.5);
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
            double rh_reaction = lh_reaction - span * uls_udl - total_partial_udl - total_points_load;

            bmdData[segments, 2] = rh_reaction;
            bmdData[segments, 1] = Math.Pow(Math.Pow(span, 2), .5);
            bmdData[segments, 3] = 0;

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////

            var deflection_positions = new double[10];

            for (int i = 1; i < deflection_positions.Length; i++)
            {
                deflection_positions[i] = i * span / 10;
            }

            for (int i = 1; i < deflection_positions.Length; i++)
            {
                double lh_unit_reaction = (span - deflection_positions[i]) / span;

                for (int j = 1; j < segments - 1; j++)
                {
                    double lever = (j * interval) - deflection_positions[i];

                    double unit_lever = lever > 0 ? lever : 0;

                    double unit_moment = lh_unit_reaction * (j * interval) - unit_lever;

                    momarray[j, i + 3] = unit_moment;
                }
            }

            //beam.WebDepthRight = 500;

            double leftheight = Math.Pow(Math.Pow(section.WebHeight, 2), 0.5);
            double rightheight = Math.Pow(Math.Pow(section.WebHeight, 2), 0.5);

            for (int j = 1; j < segments - 1; j++)
            {
                var section_position = j * interval;
                var section_depth = leftheight - (leftheight - rightheight) * section_position / span;
                momarray[j, 13] = section_depth;
            }

            for (int j = 1; j < segments - 1; j++)
            {
                momarray[j, 25] = momarray[j, 13] + 0.5 * (section.FlangeThickness + section.FlangeThickness);
            }

            double top_flg_thick = section.FlangeThickness;
            double top_flg_width = section.FlangeWidth;
            double bottom_flg_thick = section.FlangeThickness;
            double bottom_flg_width = section.FlangeWidth;
            double web = section.WebThickness;

            for (int j = 1; j < segments - 1; j++)
            {
                double web_depth = momarray[j, 13];
                var section_area = top_flg_thick * top_flg_width + bottom_flg_thick * bottom_flg_width;
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
                double distance = k * span / 10;

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
            def_sums[11, 1] = Math.Pow(Math.Pow(span, 2), 0.5);
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
                DesignType = localization.DesignType,
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
    }
}
