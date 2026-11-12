using System.Text.Json;
using WalterLife.Models;
namespace WalterLife.Services;
public sealed class PushStore
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly string directory;
    private static readonly JsonSerializerOptions options = new() { WriteIndented = true };
    public PushStore(IWebHostEnvironment env)
    {
        directory = Path.Combine(env.ContentRootPath, "App_Data");
        Directory.CreateDirectory(directory);
    }
    private string DevicesPath => Path.Combine(directory, "push-devices.json");
    private string StatePath => Path.Combine(directory, "push-deliveries.json");
    public async Task<List<PushDevice>> DevicesAsync()
    {
        await gate.WaitAsync();
        try { return await ReadAsync<List<PushDevice>>(DevicesPath) ?? new(); }
        finally { gate.Release(); }
    }
    public async Task AddAsync(PushDevice device)
    {
        await gate.WaitAsync();
        try
        {
            var devices = await ReadAsync<List<PushDevice>>(DevicesPath) ?? new();
            devices.RemoveAll(d => d.Endpoint == device.Endpoint);
            devices.Add(device);
            await WriteAsync(DevicesPath, devices);
        }
        finally { gate.Release(); }
    }
    public async Task RemoveAsync(string endpoint)
    {
        await gate.WaitAsync();
        try
        {
            var devices = await ReadAsync<List<PushDevice>>(DevicesPath) ?? new();
            devices.RemoveAll(d => d.Endpoint == endpoint);
            await WriteAsync(DevicesPath, devices);
        }
        finally { gate.Release(); }
    }
    public async Task<bool> WasSentAsync(Guid id, DateTime date)
    {
        await gate.WaitAsync();
        try
        {
            var state = await ReadAsync<PushDeliveryState>(StatePath) ?? new();
            return state.LastSent.TryGetValue(id.ToString(), out var sent) && sent == date.ToString("yyyy-MM-dd");
        }
        finally { gate.Release(); }
    }
    public async Task MarkSentAsync(Guid id, DateTime date)
    {
        await gate.WaitAsync();
        try
        {
            var state = await ReadAsync<PushDeliveryState>(StatePath) ?? new();
            state.LastSent[id.ToString()] = date.ToString("yyyy-MM-dd");
            await WriteAsync(StatePath, state);
        }
        finally { gate.Release(); }
    }
    private static async Task<T?> ReadAsync<T>(string path)
    {
        if (!File.Exists(path)) return default;
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<T>(stream, options);
    }
    private static async Task WriteAsync<T>(string path, T value)
    {
        var temp = path + ".tmp";
        await using (var stream = File.Create(temp)) await JsonSerializer.SerializeAsync(stream, value, options);
        File.Move(temp, path, true);
    }
}