using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Models.Dtos;
using Server.Services.Scheduling;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SchedulerController : ControllerBase
{
    private readonly ISchedulerOrderService _schedulerService;
    private readonly ILogger<SchedulerController> _logger;

    public SchedulerController(ISchedulerOrderService schedulerService, ILogger<SchedulerController> logger)
    {
        _schedulerService = schedulerService;
        _logger = logger;
    }

    [HttpPost("order")]
    public async Task<ActionResult<SchedulerOrderResponse>> GetRecommendedOrder([FromBody] SchedulerOrderRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var result = await _schedulerService.GetRecommendedOrderAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid scheduler request: {Message}", ex.Message);
            return Problem(
                title: "Invalid request",
                detail: ex.Message,
                statusCode: 400
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Scheduler operation failed: {Message}", ex.Message);
            return Problem(
                title: "Scheduling failed",
                detail: ex.Message,
                statusCode: 422
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in scheduler");
            return Problem(
                title: "Internal server error",
                detail: "An unexpected error occurred while processing the request",
                statusCode: 500
            );
        }
    }
}
