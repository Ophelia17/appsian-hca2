using System.ComponentModel.DataAnnotations;

namespace Server.Models.Dtos;

public class CreateFeedbackDto
{
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }
    
    [MaxLength(1000)]
    public string? Comment { get; set; }
}

public class FeedbackDto
{
    public Guid Id { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
