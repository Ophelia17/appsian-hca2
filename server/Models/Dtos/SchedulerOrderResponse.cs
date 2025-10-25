namespace Server.Models.Dtos;

public class SchedulerOrderResponse
{
    public List<string> RecommendedOrder { get; set; } = new();
    public string StrategyUsed { get; set; } = string.Empty;
}
