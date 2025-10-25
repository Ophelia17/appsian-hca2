using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Models.Dtos;
using Server.Services.Scheduling;
using Swashbuckle.AspNetCore.Annotations;

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
    [SwaggerOperation(
        Summary = "Get recommended task order",
        Description = @"Returns a dependency-aware recommended order for tasks using topological sorting with configurable tie-breaking strategies.

## Example cURL Request:
```bash
curl -X POST 'https://your-api.com/api/scheduler/order' \
  -H 'Authorization: Bearer YOUR_JWT_TOKEN' \
  -H 'Content-Type: application/json' \
  -d '{
    ""tasks"": [
      { ""title"": ""Design API"", ""estimatedHours"": 5, ""dueDate"": ""2025-10-25"", ""dependencies"": [] },
      { ""title"": ""Implement Backend"", ""estimatedHours"": 12, ""dueDate"": ""2025-10-28"", ""dependencies"": [""Design API""] },
      { ""title"": ""Build Frontend"", ""estimatedHours"": 10, ""dueDate"": ""2025-10-30"", ""dependencies"": [""Design API""] },
      { ""title"": ""End-to-End Test"", ""estimatedHours"": 8, ""dueDate"": ""2025-10-31"", ""dependencies"": [""Implement Backend"", ""Build Frontend""] }
    ],
    ""strategy"": ""DepsDueSjf""
  }'
```",
        OperationId = "GetRecommendedOrder"
    )]
    [SwaggerResponse(200, "Successfully generated recommended order", typeof(SchedulerOrderResponse))]
    [SwaggerResponse(400, "Invalid request - duplicate titles, unknown dependencies, or self-dependencies")]
    [SwaggerResponse(422, "Scheduling failed - circular dependencies detected")]
    [SwaggerResponse(401, "Unauthorized - valid JWT token required")]
    [SwaggerResponse(500, "Internal server error")]
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
