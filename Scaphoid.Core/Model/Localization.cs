namespace Scaphoid.Core.Model
{
    public class Localization
    {
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public DesignType DesignType { get; set; } = new DesignType();
        public DesignParameters DesignParameters { get; set; } = new DesignParameters();
        public DeflectionLimit DeflectionLimit { get; set; } = new DeflectionLimit();
    }

    public enum DesignType
    {
        UK = 1,
        Irish = 2,
        UserDefined = 3
    }

    public class DeflectionLimit
    {
        public double VariableLoads { get; set; } = 360;
        public double TotalLoads { get; set; } = 250;
    }

    public class DesignParameters
    {
        public double GammaG { get; set; }
        public double GammaQ { get; set; }
        public double ReductionFactorF { get; set; }
        public double ModificationFactorKflHtoBLessThanTwo { get; set; }
        public double ModificationFactorAllOtherHtoB { get; set; }
        public double SteelGradeS235LessThan16mm { get; set; }
        public double SteelGradeS235Between16and40mm { get; set; }
        public double SteelGradeS235Between40and63mm { get; set; }
        public double SteelGradeS355LessThan16mm { get; set; }
        public double SteelGradeS355Between16and40mm { get; set; }
        public double SteelGradeS355Between40and63mm { get; set; }
    }
}
