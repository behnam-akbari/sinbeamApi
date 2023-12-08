namespace Scaphoid.Core.Model
{
    public class Localization
    {
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public DesignType DesignType { get; set; }
        public DesignParameters DesignParameters { get; set; }
        public DeflectionLimit DeflectionLimit { get; set; }
        public ULSLoadExpression ULSLoadExpression { get; set; }

        public SteelType SteelType { get; set; }

        public int PsiValue { get; set; }
    }

    public enum DesignType
    {
        UK = 1,
        Irish = 2,
        Iran = 3,
        UserDefined = 4
    }

    public enum ULSLoadExpression
    {
        Expression610a = 1,
        Expression610aAnd610b = 2,
    }

    public enum ElementType
    {
        Column = 1,
        Rafter = 2
    }

    public class DeflectionLimit
    {
        public double VariableLoads { get; set; }
        public double TotalLoads { get; set; }
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
