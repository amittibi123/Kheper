using System.Net.Http.Json;
using Blazored.LocalStorage;
// Required for Timer
// Required for OrderBy
// Required for JsonSerializer
// Required for IHttpClientFactory

// Required for HttpClient



namespace set.Shared.Models;

public class SyncService : IAsyncDisposable
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ILocalStorageService _localStorage;
    private Timer? _timer;

    // UI subscribes to this — fired when server has newer data
    public event Action? OnDataChanged;

    public SyncService(IHttpClientFactory clientFactory, ILocalStorageService localStorage)
    {
        _clientFactory = clientFactory;
        _localStorage = localStorage;
    }

    // ── Start polling ──────────────────────────────────────

    public void Start()
    {
        _timer = new Timer(
            async _ => await CheckForChanges(),
            null,
            TimeSpan.FromSeconds(10),  // first check after 10s
            TimeSpan.FromSeconds(30)   // then every 30s
        );
    }

    public void Stop() => _timer?.Change(Timeout.Infinite, Timeout.Infinite);

    // ── Poll: compare server vs local ─────────────────────

    public async Task CheckForChanges()
    {
        try
        {
            var userId = await _localStorage.GetItemAsync<int?>("userId");
            if (userId == null || userId == 0) return;

            var client = _clientFactory.CreateClient("LocalApi");
            var server = await client.GetFromJsonAsync<SyncPayload>($"api/SyncAPI/get/{userId}");
            if (server == null) return;

            var localTasks    = await _localStorage.GetItemAsync<List<TaskDto>>("tasks")       ?? new();
            var localPackages = await _localStorage.GetItemAsync<List<PackageDto>>("packages") ?? new();

            bool changed = HasChanges(localTasks,    server.Tasks)
                           || HasChanges(localPackages, server.Packages);

            if (changed)
            {
                // Server wins — overwrite local
                await _localStorage.SetItemAsync("tasks",    server.Tasks);
                await _localStorage.SetItemAsync("packages", server.Packages);
                OnDataChanged?.Invoke(); // tell the UI to reload
            }
        }
        catch { /* swallow network errors silently */ }
    }

    // ── Push: local → server (call after every user change) ─

    public async Task PushToServer()
    {
        try
        {
            var userId = await _localStorage.GetItemAsync<int?>("userId");
            if (userId == null || userId == 0) return;

            var tasks    = await _localStorage.GetItemAsync<List<TaskDto>>("tasks")       ?? new();
            var packages = await _localStorage.GetItemAsync<List<PackageDto>>("packages") ?? new();

            var client = _clientFactory.CreateClient("LocalApi");
            await client.PostAsJsonAsync("api/SyncAPI/sync", new SyncRequest
            {
                UserId   = userId.Value,
                Tasks    = tasks,
                Packages = packages
            });
        }
        catch { /* swallow network errors silently */ }
    }

    // ── Change detection ───────────────────────────────────

    private bool HasChanges<T>(List<T> local, List<T> server)
    {
        if (local.Count != server.Count) return true;

        var localJson  = System.Text.Json.JsonSerializer.Serialize(
            local.OrderBy(x => System.Text.Json.JsonSerializer.Serialize(x)));
        var serverJson = System.Text.Json.JsonSerializer.Serialize(
            server.OrderBy(x => System.Text.Json.JsonSerializer.Serialize(x)));

        return localJson != serverJson;
    }

    // ── DTOs ───────────────────────────────────────────────

    public class TaskDto
    {
        public string Task        { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? DueDate  { get; set; }
        public string Path        { get; set; } = "/";
    }

    public class PackageDto
    {
        public string Name       { get; set; } = string.Empty;
        public string Path       { get; set; } = "/";
        public string ParentPath { get; set; } = "/";
    }

    public class SyncPayload
    {
        public List<TaskDto>    Tasks    { get; set; } = new();
        public List<PackageDto> Packages { get; set; } = new();
    }

    public class SyncRequest
    {
        public int               UserId   { get; set; }
        public List<TaskDto>     Tasks    { get; set; } = new();
        public List<PackageDto>  Packages { get; set; } = new();
    }

    public async ValueTask DisposeAsync()
    {
        Stop();
        if (_timer != null) await _timer.DisposeAsync();
    }
}