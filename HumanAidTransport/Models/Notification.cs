using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanAidTransport.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("VolunteerId")]
        public int VolunteerId { get; set; } 
        public string Message { get; set; } 
        public bool IsRead { get; set; } = false; 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
        public Volunteer Volunteer { get; set; }
    }
}
