// src/WebApi/Controllers/MetricsController.cs
using Microsoft.AspNetCore.Mvc;
using Prometheus;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetricsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetMetrics()
    {
        // Prometheus ya expone /metrics, este es solo un redirect
        return Redirect("/metrics");
    }
    
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", metrics = "/metrics" });
    }
}