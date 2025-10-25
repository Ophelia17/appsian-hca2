using System.Text.Json.Serialization;

namespace Server.Models.Dtos;

public class SchedulerOrderResponse
{
    [JsonPropertyName("recommendedOrder")]
    public List<string> RecommendedOrder { get; set; } = new();
    
    [JsonPropertyName("strategyUsed")]
    public string StrategyUsed { get; set; } = string.Empty;
}
