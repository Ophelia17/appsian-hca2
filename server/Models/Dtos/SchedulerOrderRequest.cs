using System.ComponentModel.DataAnnotations;

namespace Server.Models.Dtos;

public class SchedulerOrderRequest
{
    [Required]
    [MinLength(1)]
    public List<SchedulerTaskDto> Tasks { get; set; } = new();
    
    public OrderStrategy? Strategy { get; set; } = OrderStrategy.DepsDueSjf;
}

public enum OrderStrategy
{
    DepsDueSjf,    // Dependencies + Due Date + Shortest Job First
    DepsDueFifo,   // Dependencies + Due Date + First In First Out
    DepsOnly       // Dependencies only
}
