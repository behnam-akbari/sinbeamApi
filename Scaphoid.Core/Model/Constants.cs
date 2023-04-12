namespace Scaphoid.Core.Model
{
    public static class Constants
    {
        public static List<double> WebThicknessCollection = new() { 1.5, 2, 2.5, 3 };

        public static List<double> FlangeThicknessCollection = new() { 8, 10, 12, 15, 20, 25, 30, 40 };

        public static List<double> FlangeWidthCollection = new() { 160, 180, 200, 220, 250, 300, 350, 400 };

        public static readonly DesignParameters UkNA = new()
        {
            GammaG = 1.35,
            GammaQ = 1.5,
            ReductionFactorF = 0.925,
            ModificationFactorKflHtoBLessThanTwo = 1,
            ModificationFactorAllOtherHtoB = 0.9,
            SteelGradeS235LessThan16mm = 275,
            SteelGradeS235Between16and40mm = 265,
            SteelGradeS235Between40and63mm = 255,
            SteelGradeS355LessThan16mm = 355,
            SteelGradeS355Between16and40mm = 345,
            SteelGradeS355Between40and63mm = 335
        };

        public static readonly DesignParameters IrishNA = new()
        {
            GammaG = 1.35,
            GammaQ = 1.5,
            ReductionFactorF =  0.85,
            ModificationFactorKflHtoBLessThanTwo = 1.1,
            ModificationFactorAllOtherHtoB = 1.1,
            SteelGradeS235LessThan16mm = 275,
            SteelGradeS235Between16and40mm = 275,
            SteelGradeS235Between40and63mm = 255,
            SteelGradeS355LessThan16mm = 355,
            SteelGradeS355Between16and40mm = 355,
            SteelGradeS355Between40and63mm = 335
        };

        public static readonly DesignParameters DefaultNA = new()
        {
            GammaG = 1.35,
            GammaQ = 1.5,
            ReductionFactorF = 0.85,
            ModificationFactorKflHtoBLessThanTwo = 1.1,
            ModificationFactorAllOtherHtoB = 1.1,
            SteelGradeS235LessThan16mm = 275,
            SteelGradeS235Between16and40mm = 265,
            SteelGradeS235Between40and63mm = 255,
            SteelGradeS355LessThan16mm = 355,
            SteelGradeS355Between16and40mm = 345,
            SteelGradeS355Between40and63mm = 335
        };
    }
}
