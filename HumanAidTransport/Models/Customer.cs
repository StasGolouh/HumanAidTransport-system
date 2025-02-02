using HumanAidTransport.Models;
using System.ComponentModel.DataAnnotations;

public class Customer
{
    [Key]
    public int CustomerId { get; set; }

    [Required]
    [StringLength(100)]
    public string Username { get; set; }

    [Required]
    [StringLength(100)]
    public string Password { get; set; }

    public List<Order> Orders { get; set; }

    // public List<Rating> Ratings { get; set; }
}
