using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using server.Model;

namespace server.Server;

public class WebSocketHandler
{
    private WebSocket _webSocket;

    public WebSocketHandler (WebSocket webSocket)
    {
        _webSocket = webSocket;
    }
    
public async Task HandleAsync()
    {
        while (_webSocket.State == WebSocketState.Open)
        {
            try
            {
                var buffer = new byte[1024 * 4];
                var result = await _webSocket.ReceiveAsync(buffer, CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                }

                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var message = JsonSerializer.Deserialize<Message>(json);

                GameServer gameServer = GameServer.GetInstance();

                gameServer.HandlePlayer(this, message);
            }
            catch (Exception ex)
            {
                SendError(ex.Message);
            }
            _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }
    }

    public async Task SendMessageAsync(string message)
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        if (_webSocket.State == WebSocketState.Open)
        {
            await _webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    public async Task SendError(string message)
    {
        var json = JsonSerializer.Serialize(new
        {
            type = "error",
            error = message
        });
        var bytes = Encoding.UTF8.GetBytes(json);
        _webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);    }
}