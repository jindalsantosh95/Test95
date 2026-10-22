using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace VAA.EAPI.HealthStatusApp.SignalRClasses
{
    public interface ISignalRService : IAsyncDisposable
    {
        HubConnection GetConnection();
        Task StartConnectionAsync(CancellationToken cancellationToken = default);
        Task StopConnectionAsync(CancellationToken cancellationToken = default);
        bool IsConnected { get; }

        event Func<Exception?, Task>? OnConnectionClosed;
        event Func<Exception?, Task>? OnReconnecting;
        event Func<string?, Task>? OnReconnected;
    }

    public class SignalRService : ISignalRService
    {
        private readonly HubConnection _hubConnection;
        private readonly ILogger<SignalRService> _logger;
        private int _isDisposed;

        public SignalRService(NavigationManager navigationManager, ILogger<SignalRService> logger)
        {
            _logger = logger;
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(navigationManager.ToAbsoluteUri("/healthStatusHub"), options =>
                {
                    options.HttpMessageHandlerFactory = _ =>
                    {
                        var handler = new HttpClientHandler();
                        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                        return handler;
                    };
                })
                .Build();

            _hubConnection.Closed += async (ex) => await HandleConnectionClosed(ex);
            _hubConnection.Reconnecting += async (ex) => await HandleReconnecting(ex);
            _hubConnection.Reconnected += async (id) => await HandleReconnected(id);
        }

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        public HubConnection GetConnection() => _hubConnection;

        public async Task StartConnectionAsync(CancellationToken cancellationToken = default)
        {
            if (_hubConnection.State == HubConnectionState.Disconnected && _isDisposed == 0)
            {
                try
                {
                    await _hubConnection.StartAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error starting SignalR connection");
                    throw;
                }
            }
        }

        public async Task StopConnectionAsync(CancellationToken cancellationToken = default)
        {
            if (_hubConnection.State != HubConnectionState.Disconnected && _isDisposed == 0)
            {
                try
                {
                    await _hubConnection.StopAsync(cancellationToken);
                    _logger.LogInformation("SignalR connection stopped.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error stopping SignalR connection");
                    throw;
                }
            }
        }

        private async Task HandleConnectionClosed(Exception? exception)
        {
            _logger.LogWarning(exception, "SignalR connection closed.");
            if (OnConnectionClosed is not null)
            {
                await OnConnectionClosed.Invoke(exception);
            }
        }

        private async Task HandleReconnecting(Exception? exception)
        {
            _logger.LogWarning(exception, "SignalR connection reconnecting...");
            if (OnReconnecting is not null)
            {
                await OnReconnecting.Invoke(exception);
            }
        }

        private async Task HandleReconnected(string? connectionId)
        {
            if (OnReconnected is not null)
            {
                await OnReconnected.Invoke(connectionId);
            }
        }

        public event Func<Exception?, Task>? OnConnectionClosed;
        public event Func<Exception?, Task>? OnReconnecting;
        public event Func<string?, Task>? OnReconnected;

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _isDisposed, 1) == 0)
            {
                if (_hubConnection is not null)
                {
                    await StopConnectionAsync();
                    await _hubConnection.DisposeAsync();
                }
            }
        }
    }
}
