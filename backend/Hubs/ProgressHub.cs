using Microsoft.AspNetCore.SignalR;

namespace VideoProcessor.Hubs;

public class ProgressHub : Hub
{
    private readonly ILogger<ProgressHub> _logger;

    public ProgressHub(ILogger<ProgressHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"Client connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendProgress(double progress, double eta)
    {
        await Clients.All.SendAsync("progress", progress, eta);
    }

    public async Task SendCompleted(string fileId, string downloadUrl)
    {
        await Clients.All.SendAsync("completed", fileId, downloadUrl);
    }

    public async Task SendError(string message)
    {
        await Clients.All.SendAsync("error", message);
    }
}
