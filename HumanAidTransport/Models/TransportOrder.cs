    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace HumanAidTransport.Models
    {
        public class TransportOrder
        {
            [Key]
            public int OrderId { get; set; }

            [ForeignKey("DeliveryRequestId")]
            [Required]
            public int DeliveryRequestId { get; set; }

            [ForeignKey("HumanAidId")]
            public int HumanAidId { get; set; }
            public HumanitarianAid? HumanitarianAid { get; set; }

            [Required]
            public string? Name { get; set; }

            [Required]
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            public DateTime? ExpectedDeliveryTime { get; set; }

            public string PaymentStatus => Payment.HasValue ? Payment.Value.ToString("F2") : "Free";
            public double? Payment { get; set; }

            [Required]
            public string? DeliveryAddressFrom { get; set; }

            [Required]
            public string? DeliveryAddressTo { get; set; }

        }
    }
