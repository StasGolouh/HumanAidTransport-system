    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace HumanAidTransport.Models
    {
        public class TransportOrder
        {
            [Key]
            public int OrderId { get; set; }

            [ForeignKey("DeliveryRequestId")]
            public int DeliveryRequestId { get; set; }

            [ForeignKey("HumanAidId")]
            public int HumanAidId { get; set; }
            public HumanitarianAid? HumanitarianAid { get; set; }

            [Required]
            public string? Name { get; set; }
            
            [Required]
            public string? Status { get; set; }
            public DateTime? ExpectedDeliveryTime { get; set; }

            public double? Payment { get; set; }

            [Required]
            public string? DeliveryAddressFrom { get; set; }

            [Required]
            public string? DeliveryAddressTo { get; set; }

            [Required]
            public int VolunteerId { get; set; } 

        }
    }
