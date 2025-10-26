using System.ComponentModel.DataAnnotations;

namespace Server.Data.Entities;

public class UserFeedback
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }
    
    [MaxLength(1000)]
    public string? Comment { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
}
