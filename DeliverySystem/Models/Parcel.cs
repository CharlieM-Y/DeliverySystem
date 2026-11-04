namespace DeliverySystem.Models
{
    public class Parcel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Parcel dimensions in centimetres
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        // Parcel weight in kilograms
        public double Weight { get; set; }

        // Foreign key to the delivery
        public Guid DeliveryId { get; set; }

        // Navigation property
        public Delivery? Delivery { get; set; }
    }
}