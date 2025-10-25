using Microsoft.AspNetCore.Mvc;
using Server.Models.Dtos;
using Server.Services;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await _authService.RegisterAsync(dto);
        
        if (result == null)
        {
            return Problem(
                title: "Registration failed",
                detail: "Email already exists",
                statusCode: 400
            );
        }

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await _authService.LoginAsync(dto);
        
        if (result == null)
        {
            return Problem(
                title: "Authentication failed",
                detail: "Invalid email or password",
                statusCode: 401
            );
        }

        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await _authService.RefreshTokenAsync(dto.RefreshToken);
        
        if (result == null)
        {
            return Problem(
                title: "Token refresh failed",
                detail: "Invalid or expired refresh token",
                statusCode: 401
            );
        }

        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        await _authService.LogoutAsync(dto.RefreshToken);
        return NoContent();
    }
}
