using System.Text.Json;
using WalterLife.Models;
using WebPush;
namespace WalterLife.Services;

/// <summary>Sends due-date notifications once per item per scheduled date while the server is running.</summary>
public sealed class PushReminderWorker(LifeStore life, PushStore store, IConfiguration config, ILogger<PushReminderWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        do
        {
            try { await SendDueAsync(stoppingToken); }
            catch (Exception ex) { logger.LogError(ex, "Push reminder check failed"); }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task SendDueAsync(CancellationToken cancellationToken)
    {
        var publicKey = config["Push:PublicKey"];
        var privateKey = config["Push:PrivateKey"];
        var subject = config["Push:Subject"];
        if (string.IsNullOrWhiteSpace(publicKey) || string.IsNullOrWhiteSpace(privateKey) || string.IsNullOrWhiteSpace(subject)) return;
        var hour = Math.Clamp(config.GetValue("Push:Hour", 9), 0, 23);
        if (DateTime.Now.Hour < hour) return;
        var devices = await store.DevicesAsync();
        if (devices.Count == 0) return;
        var client = new WebPushClient();
        var vapid = new VapidDetails(subject, publicKey, privateKey);
        var today = DateTime.Today;
        foreach (var item in await life.GetAllAsync())
        {
            if (cancellationToken.IsCancellationRequested) break;
            if (item.Completed || item.DueDate is null || item.DueDate.Value.Date > today || await store.WasSentAsync(item.Id, item.DueDate.Value.Date)) continue;
            var payload = JsonSerializer.Serialize(new { title = "Walter Life · Reminder", body = item.Title, url = "/" });
            var delivered = false;
            foreach (var device in devices)
            {
                try
                {
                    await client.SendNotificationAsync(new PushSubscription(device.Endpoint, device.P256dh, device.Auth), payload, vapid);
                    delivered = true;
                }
                catch (WebPushException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Gone || ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await store.RemoveAsync(device.Endpoint);
                }
                catch (Exception ex) { logger.LogWarning(ex, "Failed to deliver push notification"); }
            }
            if (delivered) await store.MarkSentAsync(item.Id, item.DueDate.Value.Date);
        }
    }
}