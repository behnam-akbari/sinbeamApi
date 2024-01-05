using Scaphoid.Core.Model;

namespace Scaphoid.Infrastructure.Repositories
{
    public class CombinationRepository
    {
        public Loading GetLoading(Loading loading, CombinationType combinationType)
        {
            var output = combinationType switch
            {
                CombinationType.C1 => GetC1Loading(loading),
                CombinationType.C2 => GetC2Loading(loading),
                CombinationType.C3 => GetC3Loading(loading),
                CombinationType.C4 => GetC4Loading(loading),
                CombinationType.C5 => GetC5Loading(loading),
                _ => loading,
            };

            output.LoadType = LoadType.UltimateLoads;

            return output;
        }

        public Loading GetC1Loading(Loading loading)
        {
            double deadCoefficient = 1.4;
            double liveCoefficient = 0;
            double windCoefficient = 0;
            double snowCoefficient = 0;

            var output = GetLoading(loading, deadCoefficient, liveCoefficient, windCoefficient, snowCoefficient);

            return output;
        }

        public Loading GetC2Loading(Loading loading)
        {
            double deadCoefficient = 1.2;
            double liveCoefficient = 1.6;
            double windCoefficient = 0;
            double snowCoefficient = 0.5;

            var output = GetLoading(loading, deadCoefficient, liveCoefficient, windCoefficient, snowCoefficient);

            return output;
        }

        public Loading GetC3Loading(Loading loading)
        {
            double deadCoefficient = 1.2;
            double liveCoefficient = 0;
            double windCoefficient = 0.8;
            double snowCoefficient = 1.6;

            var output = GetLoading(loading, deadCoefficient, liveCoefficient, windCoefficient, snowCoefficient);

            return output;
        }

        public Loading GetC4Loading(Loading loading)
        {
            double deadCoefficient = 1.2;
            double liveCoefficient = 1;
            double windCoefficient = 0;
            double snowCoefficient = 1.6;

            var output = GetLoading(loading, deadCoefficient, liveCoefficient, windCoefficient, snowCoefficient);

            return output;
        }

        public Loading GetC5Loading(Loading loading)
        {
            double deadCoefficient = 1.2;
            double liveCoefficient = 1;
            double windCoefficient = 1.6;
            double snowCoefficient = 0.5;

            var output = GetLoading(loading, deadCoefficient, liveCoefficient, windCoefficient, snowCoefficient);

            return output;
        }

        private Loading GetLoading(Loading loading,
            double deadCoefficient,
            double liveCoefficient,
            double windCoefficient,
            double snowCoefficient)
        {
            var loadParameters = new LoadParameters
            {
                Udl = GetUdl(loading, deadCoefficient, liveCoefficient, windCoefficient, snowCoefficient),
                EndMomentLeft = GetEndMomentLeft(loading, deadCoefficient, liveCoefficient, windCoefficient, snowCoefficient),
                EndMomentRight = GetEndMomentRight(loading, deadCoefficient, liveCoefficient, windCoefficient, snowCoefficient),
                AxialForce = GetAxialForce(loading, deadCoefficient, liveCoefficient, windCoefficient, snowCoefficient)
            };

            var pointLoads = GetPointLoads(loading, deadCoefficient, liveCoefficient, windCoefficient, snowCoefficient);

            return new Loading
            {
                UltimateLoads = loadParameters,
                PointLoads = pointLoads
            };
        }

        private double GetUdl(Loading loading,
            double deadCoefficient,
            double liveCoefficient,
            double windCoefficient,
            double snowCoefficient)
        {
            var dead = loading.DistributeLoads.Where(e => e.Type == XType.Dead).FirstOrDefault();
            var live = loading.DistributeLoads.Where(e => e.Type == XType.Live).FirstOrDefault();
            var wind = loading.DistributeLoads.Where(e => e.Type == XType.Wind).FirstOrDefault();
            var snow = loading.DistributeLoads.Where(e => e.Type == XType.Snow).FirstOrDefault();

            var deadValue = dead is null ? 0 : dead.Value;
            var liveValue = live is null ? 0 : live.Value;
            var windValue = wind is null ? 0 : wind.Value;
            var snowValue = snow is null ? 0 : snow.Value;

            var load = deadCoefficient * deadValue +
                liveCoefficient * liveValue +
                windCoefficient * windValue +
                snowCoefficient * snowValue;

            return load;
        }

        private double GetAxialForce(Loading loading,
            double deadCoefficient,
            double liveCoefficient,
            double windCoefficient,
            double snowCoefficient)
        {
            var dead = loading.AxialForceLoads.Where(e => e.Type == XType.Dead).FirstOrDefault();
            var live = loading.AxialForceLoads.Where(e => e.Type == XType.Live).FirstOrDefault();
            var wind = loading.AxialForceLoads.Where(e => e.Type == XType.Wind).FirstOrDefault();
            var snow = loading.AxialForceLoads.Where(e => e.Type == XType.Snow).FirstOrDefault();

            var deadValue = dead is null ? 0 : dead.Value;
            var liveValue = live is null ? 0 : live.Value;
            var windValue = wind is null ? 0 : wind.Value;
            var snowValue = snow is null ? 0 : snow.Value;

            var load = deadCoefficient * deadValue +
                liveCoefficient * liveValue +
                windCoefficient * windValue +
                snowCoefficient * snowValue;

            return load;
        }

        private double GetEndMomentLeft(Loading loading,
            double deadCoefficient,
            double liveCoefficient,
            double windCoefficient,
            double snowCoefficient)
        {
            var dead = loading.EndMomentLoads.Where(e => e.Type == XType.Dead).FirstOrDefault();
            var live = loading.EndMomentLoads.Where(e => e.Type == XType.Live).FirstOrDefault();
            var wind = loading.EndMomentLoads.Where(e => e.Type == XType.Wind).FirstOrDefault();
            var snow = loading.EndMomentLoads.Where(e => e.Type == XType.Snow).FirstOrDefault();

            var deadValue = dead is null ? 0 : dead.LeftValue;
            var liveValue = live is null ? 0 : live.LeftValue;
            var windValue = wind is null ? 0 : wind.LeftValue;
            var snowValue = snow is null ? 0 : snow.LeftValue;

            var load = deadCoefficient * deadValue +
                liveCoefficient * liveValue +
                windCoefficient * windValue +
                snowCoefficient * snowValue;

            return load;
        }

        private double GetEndMomentRight(Loading loading,
            double deadCoefficient,
            double liveCoefficient,
            double windCoefficient,
            double snowCoefficient)
        {
            var dead = loading.EndMomentLoads.Where(e => e.Type == XType.Dead).FirstOrDefault();
            var live = loading.EndMomentLoads.Where(e => e.Type == XType.Live).FirstOrDefault();
            var wind = loading.EndMomentLoads.Where(e => e.Type == XType.Wind).FirstOrDefault();
            var snow = loading.EndMomentLoads.Where(e => e.Type == XType.Snow).FirstOrDefault();

            var deadValue = dead is null ? 0 : dead.RightValue;
            var liveValue = live is null ? 0 : live.RightValue;
            var windValue = wind is null ? 0 : wind.RightValue;
            var snowValue = snow is null ? 0 : snow.RightValue;

            var load = deadCoefficient * deadValue +
                liveCoefficient * liveValue +
                windCoefficient * windValue +
                snowCoefficient * snowValue;

            return load;
        }

        private List<PointLoad> GetPointLoads(Loading loading,
            double deadCoefficient,
            double liveCoefficient,
            double windCoefficient,
            double snowCoefficient)
        {
            var pointLoads = loading.XPointLoads
                .GroupBy(e => e.Position)
                .Select(e => new PointLoad
                {
                    Position = e.Key,
                    Load = GetPositionPointLoads(e.ToList(), deadCoefficient, liveCoefficient, windCoefficient, snowCoefficient),
                })
                .ToList();

            return pointLoads;
        }

        private double GetPositionPointLoads(ICollection<XPointLoad> xPointLoads,
            double deadCoefficient,
            double liveCoefficient,
            double windCoefficient,
            double snowCoefficient)
        {
            var dead = xPointLoads.Where(e => e.Type == XType.Dead).FirstOrDefault();
            var live = xPointLoads.Where(e => e.Type == XType.Live).FirstOrDefault();
            var wind = xPointLoads.Where(e => e.Type == XType.Wind).FirstOrDefault();
            var snow = xPointLoads.Where(e => e.Type == XType.Snow).FirstOrDefault();

            var deadValue = dead is null ? 0 : dead.Value;
            var liveValue = live is null ? 0 : live.Value;
            var windValue = wind is null ? 0 : wind.Value;
            var snowValue = snow is null ? 0 : snow.Value;

            var load = deadCoefficient * deadValue +
                liveCoefficient * liveValue +
                windCoefficient * windValue +
                snowCoefficient * snowValue;

            return load;
        }
    }
}
