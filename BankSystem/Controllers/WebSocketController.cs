using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BankSystem.Services;

namespace BankSystem.Controllers
{
    [ApiController]
    [Route("api/ws")]
    [Authorize]
    public class WebSocketController : ControllerBase
    {
        private readonly WebSocketHandler _webSocketHandler;

        public WebSocketController(WebSocketHandler webSocketHandler)
        {
            _webSocketHandler = webSocketHandler;
        }

        [HttpGet("account/{accountId}")]
        public async Task ConnectToAccount(Guid accountId)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                await _webSocketHandler.HandleWebSocketAsync(HttpContext, accountId);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }
    }
}