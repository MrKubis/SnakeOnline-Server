using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace server.Model;

public class ClientMessage
{
    public ClientMessageType Type { get; set; }
    public string? Content { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ClientMessageType
{
    [EnumMember(Value = "Join")]
    Join,
    [EnumMember(Value = "Quit")]
    Quit,
    [EnumMember(Value = "Move")]
    Move,
    [EnumMember(Value = "Message")]
    Message
}