using System.Text.Json;
using WalterLife.Models;
namespace WalterLife.Services;
public sealed class LifeStore
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly string path;
    private readonly JsonSerializerOptions options = new() { WriteIndented = true };
    public LifeStore(IWebHostEnvironment env)
    {
        var folder = Path.Combine(env.ContentRootPath, "App_Data");
        Directory.CreateDirectory(folder);
        path = Path.Combine(folder, "life.json");
    }
    public async Task<List<LifeItem>> GetAllAsync()
    {
        await gate.WaitAsync();
        try { return await ReadAsync(); } finally { gate.Release(); }
    }
    public async Task SaveAsync(LifeItem item)
    {
        await gate.WaitAsync();
        try
        {
            var items = await ReadAsync();
            var index = items.FindIndex(x => x.Id == item.Id);
            if (index < 0) items.Add(item); else items[index] = item;
            await WriteAsync(items);
        }
        finally { gate.Release(); }
    }
    public async Task DeleteAsync(Guid id)
    {
        await gate.WaitAsync();
        try { var items = await ReadAsync(); items.RemoveAll(x => x.Id == id); await WriteAsync(items); }
        finally { gate.Release(); }
    }
    private async Task<List<LifeItem>> ReadAsync()
    {
        if (!File.Exists(path)) return new();
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<List<LifeItem>>(stream, options) ?? new();
    }
    private async Task WriteAsync(List<LifeItem> items)
    {
        var temp = path + ".tmp";
        await using (var stream = File.Create(temp)) await JsonSerializer.SerializeAsync(stream, items, options);
        File.Move(temp, path, true);
    }
}