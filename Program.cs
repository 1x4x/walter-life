using WalterLife.Components;
using WalterLife.Models;
using WalterLife.Services;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton<LifeStore>();
builder.Services.AddSingleton<PushStore>();
builder.Services.AddHostedService<PushReminderWorker>();
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/error"); app.UseHsts(); }
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapGet("/api/push/public-key", (IConfiguration config) => Results.Ok(new { publicKey = config["Push:PublicKey"] ?? "" }));
app.MapPost("/api/push/subscribe", async (PushSubscriptionRequest request, PushStore store) =>
{
    if (request.Endpoint is null || request.Keys?.P256dh is null || request.Keys.Auth is null ||
        request.Endpoint.Length > 2048 || request.Keys.P256dh.Length > 512 || request.Keys.Auth.Length > 512 ||
        !Uri.TryCreate(request.Endpoint, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
        return Results.BadRequest();
    await store.AddAsync(new PushDevice { Endpoint = request.Endpoint, P256dh = request.Keys.P256dh, Auth = request.Keys.Auth });
    return Results.Ok();
});
app.MapPost("/api/push/unsubscribe", async (UnsubscribeRequest request, PushStore store) =>
{
    if (string.IsNullOrWhiteSpace(request.Endpoint)) return Results.BadRequest();
    await store.RemoveAsync(request.Endpoint);
    return Results.Ok();
});
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
public sealed record PushSubscriptionRequest(string? Endpoint, PushKeys? Keys);
public sealed record PushKeys(string? P256dh, string? Auth);
public sealed record UnsubscribeRequest(string? Endpoint);