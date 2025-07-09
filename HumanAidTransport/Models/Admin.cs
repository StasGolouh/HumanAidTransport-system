using System.ComponentModel.DataAnnotations;

namespace HumanAidTransport.Models
{
    public class Admin
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public string? Password { get; set; }

        [Required]
        public string ProfilePhotoURL { get; set; } = "/images/profile_photos/photoadmin.png";

        public List<Carrier> Carriers { get; set; } = new List<Carrier>();
        public List<Volunteer> Volunteers { get; set; } = new List<Volunteer>();
    }
}
