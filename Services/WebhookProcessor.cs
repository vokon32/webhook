using System.Threading.Channels;
using Webhook.Models;

namespace Webhook.Services
{
    internal sealed class WebhookProcessor(IServiceScopeFactory scopeFactory, Channel<WebhookDispatch> channel) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var dispatch in channel.Reader.ReadAllAsync(stoppingToken))
            {

                using IServiceScope scope = scopeFactory.CreateScope();

                var dispatcher = scope.ServiceProvider.GetRequiredService<WebhookDispatcher>();

                await dispatcher.ProcessAsync(dispatch.eventType, dispatch.Data);
            }
        }
    }
}
