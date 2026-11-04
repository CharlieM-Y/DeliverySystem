namespace DeliverySystem.Models
{
    public class Delivery
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Customer relationship
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        // Current delivery status
        public DeliveryStatus Status { get; set; }
            = DeliveryStatus.Pending;

        // When the delivery was created
        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        // Parcels belonging to this delivery
        public List<Parcel> Parcels { get; set; } = new();

        // Assigned delivery staff
        public Guid? DeliveryStaffId { get; set; }
        public DeliveryStaff? DeliveryStaff { get; set; }
    }
}