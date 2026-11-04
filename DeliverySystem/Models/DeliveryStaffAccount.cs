namespace DeliverySystem.Models
{
    public class DeliveryStaff : Account
    {
        public bool IsAvailable { get; set; }

        public string? LicenceNumber { get; set; }   
    }
}
