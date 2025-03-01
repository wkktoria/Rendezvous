using System.ComponentModel.DataAnnotations;

namespace Rendezvous.API.DTOs;

public class RegisterDto
{
    [Required]
    public required string Username { get; set; }

    [Required]
    public required string KnownAs { get; set; }

    [Required]
    public required string Gender { get; set; }

    [Required]
    public required string DateOfBirth { get; set; }

    [Required]
    public required string City { get; set; }

    [Required]
    public required string Country { get; set; }

    [Required]
    [StringLength(255, MinimumLength = 6)]
    public required string Password { get; set; }
}
