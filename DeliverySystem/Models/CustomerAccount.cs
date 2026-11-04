namespace DeliverySystem.Models
{
    public class Customer : Account
    {
        public string? DeliveryAddress { get; set; }

        // All deliveries belonging to this customer
        public List<Delivery> Deliveries { get; set; } = new();
    }
}