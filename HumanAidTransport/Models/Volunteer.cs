using System.ComponentModel.DataAnnotations;

public class Volunteer
{
    [Key]
    public int VolunteerId { get; set; }

    [Required]
    [StringLength(100)]
    public string Username { get; set; }

    [Required]
    [StringLength(100)]
    public string Password { get; set; }


}
