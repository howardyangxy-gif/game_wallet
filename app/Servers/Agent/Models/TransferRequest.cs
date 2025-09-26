using app.Common;

public class TransferRequest : AgentRequestBase
{
    [System.Text.Json.Serialization.JsonPropertyName("acc")]
    public string name { get; set; } = string.Empty;
    public decimal amount { get; set; }
    public string currency { get; set; } = string.Empty;
    public string orderId { get; set; } = string.Empty;
}