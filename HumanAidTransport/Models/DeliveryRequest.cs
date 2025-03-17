using HumanAidTransport.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class DeliveryRequest
{
    [Key]
    public int DeliveryRequestId { get; set; }

    [ForeignKey("CarrierId")]
    public int CarrierId { get; set; }
    public Carrier? Carrier { get; set; }

    public int? CarrierRating { get; set; }

    public string? CarrierContacts { get; set; }

    public string? VehicleName { get; set; }
    public string? VehicleModel { get; set; }
    public string? VehicleNumber { get; set; }

    [ForeignKey("HumanAidId")]
    public int HumanAidId { get; set; }
    public HumanitarianAid? HumanitarianAid { get; set; }

    public string? HumanAidName { get; set; }

    [ForeignKey("VolunteerId")]
    public int? VolunteerId { get; set; }
    public Volunteer? Volunteer { get; set; }

    public int Capacity { get; set; } 
    public string? Dimensions { get; set; } 
}
