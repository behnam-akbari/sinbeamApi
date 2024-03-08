
namespace Scaphoid.Core.Model
{
    public class Request
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int CountryId { get; set; }
        public Country Country { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
