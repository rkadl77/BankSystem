using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankSystem.Monitoring.Controllers;

[ApiController]
[Route("")]
[AllowAnonymous]
public class DashboardController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

    public DashboardController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var filePath = Path.Combine(_environment.WebRootPath, "index.html");
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("Dashboard file not found");
        }

        var content = System.IO.File.ReadAllText(filePath);
        return Content(content, "text/html");
    }
}
