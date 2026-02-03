using server.Server;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();

        var webSocketHandler = new WebSocketHandler(webSocket);
        _ = Task.Run(async () =>
        {
            try
            {
                await webSocketHandler.HandleAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        });
    }
});

app.UseWebSockets();


app.Run();