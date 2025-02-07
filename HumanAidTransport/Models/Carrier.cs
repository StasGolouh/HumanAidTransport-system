using System.ComponentModel.DataAnnotations;

public class Carrier
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string? Name { get; set; }

    [Required]
    public string? Password { get; set; }

    [Required]
    [RegularExpression(@"^\+?[0-9]{1,4}?[ -]?[0-9]{1,3}[ -]?[0-9]{1,4}[ -]?[0-9]{1,4}$", ErrorMessage = "Невірний формат номера телефону.")]
    public string? Contacts { get; set; }

    [Required]
    public string? VehicleName { get; set; }
    [Required]
    public string? VehicleModel { get; set; }

    [Required]
    [RegularExpression(@"^[A-Z0-9-]+$", ErrorMessage = "Некоректний номерний знак")]
    public string? VehicleNumber { get; set; }


    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public double? Rating { get; set; } = 1;

    public string ProfilePhotoURL { get; set; } = "~/images/defaultAvatar.png";
}
