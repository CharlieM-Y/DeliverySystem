namespace DeliverySystem.Models
{
    public class DeliveryStaff : Account
    {
        public bool IsAvailable { get; set; } = false;

        public string? LicenceNumber { get; set; }   
    }
}
