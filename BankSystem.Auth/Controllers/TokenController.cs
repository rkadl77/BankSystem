using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace BankSystem.Auth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> GetToken([FromBody] TokenRequest request)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "curl.exe",
                    Arguments = $"-X POST http://localhost:5004/connect/token -H \"Content-Type: application/x-www-form-urlencoded\" -d \"client_id=bank.password&grant_type=password&username={request.Username}&password={request.Password}&scope=openid profile bank.api\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var response = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            if (!string.IsNullOrEmpty(error) && !error.Contains("Total"))
            {
                return BadRequest(error);
            }

            return Ok(response);
        }
    }

    public class TokenRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}