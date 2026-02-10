using server.Server;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();

        var webSocketHandler = new WebSocketHandler(webSocket);
        try
        {
            await webSocketHandler.HandleAsync();
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
});
app.UseWebSockets();
app.Run();