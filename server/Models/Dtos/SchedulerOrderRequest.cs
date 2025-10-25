using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Server.Models.Dtos;

public class SchedulerOrderRequest
{
    [Required]
    [MinLength(1)]
    [JsonPropertyName("tasks")]
    public List<SchedulerTaskDto> Tasks { get; set; } = new();
    
    [JsonPropertyName("strategy")]
    public OrderStrategy? Strategy { get; set; } = OrderStrategy.DepsDueSjf;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderStrategy
{
    DepsDueSjf,    // Dependencies + Due Date + Shortest Job First
    DepsDueFifo,   // Dependencies + Due Date + First In First Out
    DepsOnly       // Dependencies only
}
