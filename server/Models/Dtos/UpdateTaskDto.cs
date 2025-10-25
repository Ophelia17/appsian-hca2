using System.ComponentModel.DataAnnotations;

namespace Server.Models.Dtos;

public class UpdateTaskDto
{
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public DateTime? DueDate { get; set; }
    
    public bool IsCompleted { get; set; }
}
