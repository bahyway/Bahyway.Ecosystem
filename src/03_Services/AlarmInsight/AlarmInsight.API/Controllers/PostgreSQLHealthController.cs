using AlarmInsight.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlarmInsight.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostgreSQLHealthController : ControllerBase
{
    private readonly IPostgreSQLHealthService _healthService;

    public PostgreSQLHealthController(IPostgreSQLHealthService healthService)
    {
        _healthService = healthService;
    }

    [HttpGet("test")]
    public async Task<IActionResult> Test()
    {
        var health = await _healthService.GetClusterHealthAsync();
        return Ok(health);
    }
}