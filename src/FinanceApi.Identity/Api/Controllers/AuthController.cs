using FinanceApi.Identity.Api.Dtos;
using FinanceApi.Identity.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApi.Identity.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        logger.LogInformation("[IDENTITY] Register attempt — email={Email}", request.Email);
        try
        {
            var response = await authService.RegisterAsync(request);
            logger.LogInformation("[IDENTITY] Register successful — email={Email} userId={UserId}", response.Email, response.UserId);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("[IDENTITY] Register failed — email={Email} reason={Reason}", request.Email, ex.Message);
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        logger.LogInformation("[IDENTITY] Login attempt — email={Email}", request.Email);
        try
        {
            var response = await authService.LoginAsync(request);
            logger.LogInformation("[IDENTITY] Login successful — email={Email} userId={UserId}", response.Email, response.UserId);
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            logger.LogWarning("[IDENTITY] Login failed — email={Email} reason=invalid credentials", request.Email);
            return Unauthorized(new { error = "Invalid credentials." });
        }
    }

    [HttpPost("admin")]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminRequest request)
    {
        logger.LogInformation("[IDENTITY] CreateAdmin attempt — email={Email}", request.Email);
        try
        {
            var response = await authService.CreateAdminAsync(request);
            logger.LogInformation("[IDENTITY] Admin created — email={Email} userId={UserId}", response.Email, response.UserId);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning("[IDENTITY] CreateAdmin failed — invalid master key");
            return Unauthorized(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("[IDENTITY] CreateAdmin failed — email={Email} reason={Reason}", request.Email, ex.Message);
            return Conflict(new { error = ex.Message });
        }
    }
}
