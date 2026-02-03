using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace server.Model;

public class ClientMessage
{
    public ClientMessageType Type { get; set; }
    public string Content { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ClientMessageType
{
    [EnumMember(Value = "JOIN")]
    JOIN,
    [EnumMember(Value = "QUIT")]
    QUIT,
    [EnumMember(Value = "MOVE")]
    MOVE,
    [EnumMember(Value = "MESSAGE")]
    MESSAGE
}