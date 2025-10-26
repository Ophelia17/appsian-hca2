using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Data.Entities;
using Server.Models.Dtos;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FeedbackController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FeedbackController> _logger;

    public FeedbackController(ApplicationDbContext context, ILogger<FeedbackController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<FeedbackDto>> CreateFeedback([FromBody] CreateFeedbackDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userId = GetUserId();

        var feedback = new UserFeedback
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow
        };

        _context.UserFeedbacks.Add(feedback);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} submitted feedback with rating {Rating}", userId, dto.Rating);

        return Ok(new FeedbackDto
        {
            Id = feedback.Id,
            Rating = feedback.Rating,
            Comment = feedback.Comment,
            CreatedAt = feedback.CreatedAt
        });
    }

    [HttpGet]
    public async Task<ActionResult<List<FeedbackDto>>> GetFeedback()
    {
        var userId = GetUserId();

        var feedback = await _context.UserFeedbacks
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new FeedbackDto
            {
                Id = f.Id,
                Rating = f.Rating,
                Comment = f.Comment,
                CreatedAt = f.CreatedAt
            })
            .ToListAsync();

        return Ok(feedback);
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID");
        }
        return userId;
    }
}
