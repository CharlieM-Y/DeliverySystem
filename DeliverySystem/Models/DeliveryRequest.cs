
namespace DeliverySystem.Models
{
    public class DeliveryRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Customer who submitted the request
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        // Request information
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DeliveryRequestStatus Status { get; set; }
            = DeliveryRequestStatus.PendingReview;

        // Pickup and delivery locations
        public string PickupAddress { get; set; } = "";
        public string DeliveryAddress { get; set; } = "";

        // Parcels included in the request
        public List<Parcel> Parcels { get; set; } = new();

        // Quote prepared by Office Staff
        public decimal? QuotedCost { get; set; }

        public DateTime? EstimatedPickupTime { get; set; }
        public DateTime? EstimatedDeliveryTime { get; set; }

        // Actual delivery created after acceptance
        public Guid? DeliveryId { get; set; }
        public Delivery? Delivery { get; set; }
    }
}
