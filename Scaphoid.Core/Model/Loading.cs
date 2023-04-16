namespace Scaphoid.Core.Model
{
    public class Loading
    {
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public LoadType LoadType { get; set; }
        public LoadParameters PermanentLoads { get; set; }
        public LoadParameters VariableLoads { get; set; }
        public LoadParameters UltimateLoads { get; set; }
        public ICollection<PointLoad> PointLoads { get; set; }
    }

    public class PointLoad
    {
        public int Id { get; set; }
        public int LoadingId { get; set; }
        public Loading Loading { get; set; }
        public double Position { get; set; }
        public double Load { get; set; }
        public double PermanentAction { get; set; }
        public double VariableAction { get; set; }
    }

    public enum LoadType
    {
        CharacteristicLoads = 1,
        UltimateLoads = 2
    }

    public class LoadParameters
    {
        public int Udl { get; set; }

        public int PartialUdl { get; set; }
        public int PartialUdlStart { get; set; }
        public int PartialUdlEnd { get; set; }

        public int EndMomentLeft { get; set; }
        public int EndMomentRight { get; set; }
        public int AxialForce { get; set; }
    }
}