using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Threading.Channels;
using Webhook.Data;
using Webhook.Models;

namespace Webhook.Services
{
    internal sealed class WebhookDispatcher(Channel<WebhookDispatch> webhooksChannel, IHttpClientFactory httpClientFactory, WebhooksDbContext context)
    {

        public async Task DispatchAsync<T>(string eventType, T data) where T : notnull
        {

            await webhooksChannel.Writer.WriteAsync(new WebhookDispatch(eventType, data));
        }
        public async Task ProcessAsync<T>(string eventType, T data)
        {
            var subscriptions = await context.WebhookSubscriptions.AsNoTracking().Where(s => s.EventType == eventType).ToListAsync();

            foreach (var subscription in subscriptions)
            {
                using var httpClient = httpClientFactory.CreateClient();
                var payload = new WebhookPayload<T>
                {
                    Id = Guid.NewGuid(),
                    EventType = subscription.EventType,
                    SubsriptionId = subscription.Id,
                    Timestamp = DateTime.Now,
                    Data = data
                };

                var jsonPayload = JsonSerializer.Serialize(payload);

                try
                {
                    var response = await httpClient.PostAsJsonAsync(subscription.WebhookUrl, payload);


                    var attempt = new WebhookDeliveryAttempt
                    {
                        Id = Guid.NewGuid(),
                        WebhookSubscriptionId = subscription.Id,
                        Payload = jsonPayload,
                        ResponseStatusCode = (int)response.StatusCode,
                        Success = response.IsSuccessStatusCode,
                        TimeStamp = DateTime.UtcNow
                    };

                    await context.WebhookDeliveryAttempts.AddAsync(attempt);

                    await context.SaveChangesAsync();
                }
                catch (Exception)
                {

                    var attempt = new WebhookDeliveryAttempt
                    {
                        Id = Guid.NewGuid(),
                        WebhookSubscriptionId = subscription.Id,
                        Payload = jsonPayload,
                        ResponseStatusCode = null,
                        Success = false,
                        TimeStamp = DateTime.UtcNow
                    };

                    await context.WebhookDeliveryAttempts.AddAsync(attempt);

                    await context.SaveChangesAsync();
                }


            }
        }
    }


}
