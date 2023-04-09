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
    }
}
