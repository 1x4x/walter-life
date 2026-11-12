namespace WalterLife.Models;
public sealed class PushDevice
{
    public string Endpoint { get; set; } = "";
    public string P256dh { get; set; } = "";
    public string Auth { get; set; } = "";
}
public sealed class PushDeliveryState
{
    public Dictionary<string, string> LastSent { get; set; } = new();
}