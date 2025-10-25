using System.ComponentModel.DataAnnotations;

namespace Server.Data.Entities;

public class Project
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    
    [Required]
    [MinLength(3)]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
