namespace Scaphoid.Core.Model
{
    public class FlangeRestraint
    {
        public bool IsTopFlangeFullRestraint { get; set; }
        public List<double> TopFlangeRestraints { get; set; }

        public bool IsBottomFlangeFullRestraint { get; set; }
        public List<double> BottomFlangeRestraints { get; set; }
    }
}