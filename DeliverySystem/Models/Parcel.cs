
namespace DeliverySystem.Models
{
    public class Parcel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Dimensions in centimetres
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        // Weight in kilograms
        public double Weight { get; set; }

        // Associated delivery (if accepted)
        public Guid? DeliveryId { get; set; }
        public Delivery? Delivery { get; set; }

        // Associated delivery request
        public Guid? DeliveryRequestId { get; set; }
        public DeliveryRequest? DeliveryRequest { get; set; }
    }
}
