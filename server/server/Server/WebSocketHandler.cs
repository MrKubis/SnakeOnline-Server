using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Connections;
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
        var buffer = new byte[1024 * 4];

        try
        {

            while (_webSocket.State == WebSocketState.Open)
            {
                try
                {
                    var result = await _webSocket.ReceiveAsync(buffer, CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    }

                    var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var message = JsonSerializer.Deserialize<ClientMessage>(json);

                    GameServer gameServer = GameServer.GetInstance();

                    gameServer.HandlePlayer(this, message);

                }
                catch (Exception ex)
                {
                    await SendErrorAsync(ex.Message);
                }
            }
            GameServer gs = GameServer.GetInstance();

            gs.RemoveWebSocketHandler(this);

        }
        catch (WebSocketException ex)
        {
            
            GameServer gameServer = GameServer.GetInstance();

            gameServer.RemoveWebSocketHandler(this);
            
            Console.WriteLine(ex.GetType().Name);
        }
    }

    public async Task SendMessageAsync(ServerMessage serverMessage)
    {
        var json = JsonSerializer.Serialize(serverMessage);
        var bytes = Encoding.UTF8.GetBytes(json);
        if (_webSocket.State == WebSocketState.Open)
        {
            await _webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    public async Task SendErrorAsync(string errorMessage)
    {
        var json = JsonSerializer.Serialize(new ServerMessage
        {
            Type = ServerMessageType.Error,
            Content = errorMessage
        });
        var bytes = Encoding.UTF8.GetBytes(json);
        await _webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);    }
}