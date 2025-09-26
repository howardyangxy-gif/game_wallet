using app.Common;

public class LoginRequest : AgentRequestBase
{
    [System.Text.Json.Serialization.JsonPropertyName("acc")]
    public string name { get; set; } = string.Empty;
    public string currency { get; set; } = string.Empty;
}