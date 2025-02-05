﻿using System.ComponentModel.DataAnnotations;
using HumanAidTransport.Models;

public class Carrier: Person
{

    [Required]
    [RegularExpression(@"^\+?[0-9]{1,4}?[ -]?[0-9]{1,3}[ -]?[0-9]{1,4}[ -]?[0-9]{1,4}$",ErrorMessage = "Невірний формат номера телефону.")]
    public string Contacts { get; set; }

    [Required]
    public string VehicleName { get; set; } 
    [Required]
    public string VehicleModel { get; set; } 

    [Required]
    [RegularExpression(@"^[A-Z0-9-]+$", ErrorMessage = "Некоректний номерний знак")]
    public string VehicleNumber { get; set; }


    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public double Rating { get; set; } = 0;
}
