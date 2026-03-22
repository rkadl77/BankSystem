using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace BankSystem.Services
{
    public class WebSocketHandler
    {
        private readonly ConcurrentDictionary<Guid, List<WebSocket>> _accountSubscriptions = new();
        private readonly ILogger<WebSocketHandler> _logger;

        public WebSocketHandler(ILogger<WebSocketHandler> logger)
        {
            _logger = logger;
        }

        public async Task HandleWebSocketAsync(HttpContext context, Guid accountId)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;
                return;
            }

            var webSocket = await context.WebSockets.AcceptWebSocketAsync();

            _accountSubscriptions.AddOrUpdate(accountId,
                new List<WebSocket> { webSocket },
                (_, list) => { list.Add(webSocket); return list; });

            _logger.LogInformation("WebSocket connected for account {AccountId}", accountId);

            try
            {
                var buffer = new byte[1024 * 4];
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WebSocket error for account {AccountId}", accountId);
            }
            finally
            {
                if (_accountSubscriptions.TryGetValue(accountId, out var subscriptions))
                {
                    subscriptions.Remove(webSocket);
                    if (subscriptions.Count == 0)
                    {
                        _accountSubscriptions.TryRemove(accountId, out _);
                    }
                }

                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                _logger.LogInformation("WebSocket disconnected for account {AccountId}", accountId);
            }
        }

        public async Task NotifyAccountUpdateAsync(Guid accountId, string message)
        {
            if (!_accountSubscriptions.TryGetValue(accountId, out var subscriptions))
                return;

            var bytes = Encoding.UTF8.GetBytes(message);
            var tasks = subscriptions
                .Where(ws => ws.State == WebSocketState.Open)
                .Select(ws => ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None));

            await Task.WhenAll(tasks);
            _logger.LogInformation("Notified {Count} clients for account {AccountId}", subscriptions.Count, accountId);
        }
    }
}