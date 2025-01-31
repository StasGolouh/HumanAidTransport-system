using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HumanAidTransport.Models;

public class Carrier
{
    [Key]
    public int CarrierId { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Password { get; set; }

    [Required]
    public string ContactInfo { get; set; }

    // Додані поля для інформації про транспортний засіб
    [Required]
    public string VehicleName { get; set; }  // Назва машини

    [Required]
    public string VehicleModel { get; set; } // Модель машини

    [Required]
    [RegularExpression(@"^[A-Z0-9-]+$", ErrorMessage = "Некоректний номерний знак")]
    public string VehicleNumber { get; set; } // Номер машини

    public ICollection<Order> Orders { get; set; }

    public double Rating { get; set; } = 0;

    public int RatingCount { get; set; } = 0;
}
