using System.ComponentModel.DataAnnotations;

namespace Server.Models.Dtos;

public class SchedulerTaskDto
{
    [Required]
    [MinLength(1)]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    public double? EstimatedHours { get; set; }
    
    public DateOnly? DueDate { get; set; }
    
    public List<string> Dependencies { get; set; } = new();
}
