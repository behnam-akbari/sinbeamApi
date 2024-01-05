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
    }
}
