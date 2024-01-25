using Microsoft.AspNetCore.Mvc;
using Scaphoid.Core.Model;

namespace Schaphoid.Api.Controllers
{
    public class BaseController : ControllerBase
    {
        protected static double[,] GetArrayPoints(Localization localization, Loading loading, double gamma_g, double gamma_q, double gamma_g_610a, double gamma_q_610a)
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

        protected HelpData GetHelpData(Order order, WebSection section)
            => GetHelpData2(order, section, order.Loading);

        protected HelpData GetHelpData2(Order order, WebSection section, Loading loading)
        {
            var localization = order.Localization;
            var span = order.Span;

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

            var flanges_area = 2 * (section.FlangeThickness * section.FlangeWidth);

            var web_eff_thick = section.WebThickness;

            var ave_web_depth = section.WebHeight;

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

            var udl_moment = uls_udl * span * span / 2;

            double points_moment = 0;
            double sls_points_moment = 0;
            double unfactored_points_moment = 0;

            double[,] arraypoints = GetArrayPoints(localization, loading, gamma_g, gamma_q, gamma_g_610a, gamma_q_610a);

            for (int i = 0; i < loading.PointLoads.Count; i++)
            {
                var mom_contrib = arraypoints[i, 4] * (span - arraypoints[i, 1]);
                var sls_mom_contrib = arraypoints[i, 3] * (span - arraypoints[i, 1]);
                var unfactored_mom_contrib = (arraypoints[i, 2] + arraypoints[i, 3]) * (span - arraypoints[i, 1]);

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

            var part_udl_moment = part_uls_udl * (part_udl_end - part_udl_start) * (span - 0.5 * (part_udl_start + part_udl_end));

            var lh_reaction = (udl_moment + points_moment + part_udl_moment - uls_right_mom - uls_left_mom) / span;


            double total_points_load = 0;

            for (int i = 0; i < loading.PointLoads.Count; i++)
            {
                total_points_load = total_points_load + arraypoints[i, 4];
            }

            double total_partial_udl = part_uls_udl * (part_udl_end - part_udl_start);
            double rh_reaction = lh_reaction - span * uls_udl - total_partial_udl - total_points_load;

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
                gamma_g = gamma_g,
                gamma_q = gamma_q,
                gamma_g_610a = gamma_g_610a,
                gamma_q_610a = gamma_q_610a,
            };

            return helpData;
        }

        protected List<string> GetBottomFlangeCaptions(double[,] ltbbottom, double aeff_bottom, double bottom_flange_tension_resi, double bottom_axial_force, int seg_no)
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

        protected List<string> GetBottomFlangeMaxPositionCaptions(string max_onerous, double[,] ltbbottom, double aeff_bottom, double bottom_flange_tension_resi, double bottom_axial_force, int max_position)
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

        protected X GetBottomFlangeMaxPositionCaptions2(string max_onerous, double[,] ltbbottom, double aeff_bottom, double bottom_flange_tension_resi, double bottom_axial_force, int max_position)
        {
            var x = new X();

            x.KeyValues.Add($"Segment",  $"from {Math.Round(ltbbottom[max_position - 1, 1], 3)} m to {Math.Round(ltbbottom[max_position, 1], 3)} m");
            x.KeyValues.Add($"Segment length", $"{Math.Round(ltbbottom[max_position, 2] * 1000, 0)} mm");

            if (ltbbottom[max_position, 29] == 1)
            {
                x.KeyValues.Add($"Correction factor kct",  $"{Math.Round(ltbbottom[max_position, 10], 2)}");
                x.KeyValues.Add($"Slenderness",  $"{Math.Round(ltbbottom[max_position, 12], 3)}");
                x.KeyValues.Add($"Effective area",  $"{Math.Round(aeff_bottom, 0)} mm2");
                x.KeyValues.Add($"Resistance",  $"{Math.Round(ltbbottom[max_position, 15], 0)} kN");
                x.KeyValues.Add($"Force due to moment",  $"{Math.Round(ltbbottom[max_position, 19], 0)}  kN");
                x.KeyValues.Add($"Force from axial",  $"{Math.Round(bottom_axial_force, 0)} kN");
                x.KeyValues.Add($"Total",  $"{Math.Round(ltbbottom[max_position, 21], 0)} kN");
                x.KeyValues.Add($"Utilisation",  $"{Math.Round(ltbbottom[max_position, 23], 2)}");
            }
            else if (ltbbottom[max_position, 29] == -1)
            {
                x.KeyValues.Add($"Force due to moment", $"{Math.Round(ltbbottom[max_position, 24], 0)} kN");
                x.KeyValues.Add($"Force from axial",  $"{Math.Round(bottom_axial_force, 0)} kN");
                x.KeyValues.Add($"Totalt",  $"{Math.Round(ltbbottom[max_position, 26], 0)} kN");
                x.Captions.Add("Segment is in tension");
                x.KeyValues.Add($"Flange tension resistance",  $"{Math.Round(bottom_flange_tension_resi, 0)} kN");
            }
            else
            {
                x.Captions.Add("Segment is partially in tension and part in compression");
                x.KeyValues.Add($"Most onerous utilisation",  $"{Math.Round(ltbbottom[max_position, 30], 2)} considering {max_onerous}");
                x.KeyValues.Add($"Force from axial",  $"{Math.Round(bottom_axial_force, 0)} kN");
            }

            return x;
        }
    }
}
