using System.ComponentModel.DataAnnotations;
using HumanAidTransport.Models;

public class Volunteer
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Password { get; set; }

    [Required]
    public string ProfilePhotoURL { get; set; } = "/images/profile_photos/photodef.jpg";

    public List<HumanitarianAid> Tasks { get; set; } = new List<HumanitarianAid>();

}
