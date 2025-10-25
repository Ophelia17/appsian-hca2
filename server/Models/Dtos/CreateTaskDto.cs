using System.ComponentModel.DataAnnotations;

namespace Server.Models.Dtos;

public class CreateTaskDto
{
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public DateTime? DueDate { get; set; }
}
