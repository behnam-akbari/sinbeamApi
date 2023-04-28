namespace Scaphoid.Core.Model
{
    public class Order
    {
        public int Id { get; set; }
        public string Company { get; set; }
        public string Project { get; set; }
        public string Beam { get; set; }
        public string Designer { get; set; }
        public string Note { get; set; }
        public DateTime OrderDate { get; set; }

        public Localization Localization { get; set; }
        public Beam BeamInfo { get; set; }
        public Loading Loading { get; set; }
        public Restraint Restraint { get; set; }
    }

    public class Restraint
    {
        public int OrderId { get; set; }
        public Order Order { get; set; }

        public bool FullRestraintTopFlange { get; set; }
        public List<double> TopFlangeRestraints { get; set; }

        public bool FullRestraintBottomFlange { get; set; }
        public List<double> BottomFlangeRestraints { get; set; }
    }
}
