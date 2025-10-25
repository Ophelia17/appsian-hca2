using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Server.Models.Dtos;

public class SchedulerTaskDto
{
    [Required]
    [MinLength(1)]
    [MaxLength(200)]
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    
    [JsonPropertyName("estimatedHours")]
    public double? EstimatedHours { get; set; }
    
    [JsonPropertyName("dueDate")]
    public DateOnly? DueDate { get; set; }
    
    [JsonPropertyName("dependencies")]
    public List<string> Dependencies { get; set; } = new();
}
