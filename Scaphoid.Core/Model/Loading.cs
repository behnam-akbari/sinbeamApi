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

        public ICollection<DistributeLoad> DistributeLoads { get; set; } = new HashSet<DistributeLoad>();
        public ICollection<EndMomentLoad> EndMomentLoads { get; set; } = new HashSet<EndMomentLoad>();
        public ICollection<AxialForceLoad> AxialForceLoads { get; set; } = new HashSet<AxialForceLoad>();
        public ICollection<XPointLoad> XPointLoads { get; set; } = new HashSet<XPointLoad>();

        public CombinationType CombinationType { get; set; }
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
        public double Udl { get; set; }

        public int PartialUdl { get; set; }
        public int PartialUdlStart { get; set; }
        public int PartialUdlEnd { get; set; }

        public double EndMomentLeft { get; set; }
        public double EndMomentRight { get; set; }
        public double AxialForce { get; set; }
    }

    public enum XType
    {
        Dead = 1,
        Live = 2,
        Wind = 3,
        Snow = 4
    }

    public class DistributeLoad
    {
        public int Id { get; set; }
        public int LoadingId { get; set; }
        public Loading Loading { get; set; }
        public XType Type { get; set; }
        public int Value { get; set; }
    }

    public class EndMomentLoad
    {
        public int Id { get; set; }
        public int LoadingId { get; set; }
        public Loading Loading { get; set; }
        public XType Type { get; set; }
        public int LeftValue { get; set; }
        public int RightValue { get; set; }
    }

    public class AxialForceLoad
    {
        public int Id { get; set; }
        public int LoadingId { get; set; }
        public Loading Loading { get; set; }
        public XType Type { get; set; }
        public int Value { get; set; }
    }

    public class XPointLoad
    {
        public int Id { get; set; }
        public int LoadingId { get; set; }
        public Loading Loading { get; set; }
        public XType Type { get; set; }
        public double Position { get; set; }
        public int Value { get; set; }
    }

    public enum CombinationType
    {
        C1 = 1,
        C2 = 2,
        C3 = 3,
        C4 = 4,
        C5 = 5
    }
}