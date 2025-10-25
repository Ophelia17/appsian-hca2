using System.ComponentModel.DataAnnotations;

namespace Server.Models.Dtos;

public class CreateProjectDto
{
    [Required]
    [MinLength(3)]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}
