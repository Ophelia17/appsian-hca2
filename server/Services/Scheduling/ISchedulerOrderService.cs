using Server.Models.Dtos;

namespace Server.Services.Scheduling;

public interface ISchedulerOrderService
{
    Task<SchedulerOrderResponse> GetRecommendedOrderAsync(SchedulerOrderRequest request);
}
