using System.ComponentModel.DataAnnotations;

namespace Server.Models.Dtos;

public class RefreshTokenDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
