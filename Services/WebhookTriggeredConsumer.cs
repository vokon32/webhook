using MassTransit;
using System.Text.Json;
using Webhook.Models;
using Webhook.Data;

namespace Webhook.Services
{
    internal sealed class WebhookTriggeredConsumer(IHttpClientFactory httpClientFactory, WebhooksDbContext dbContext) : IConsumer<WebhookTriggered>
    {
        public async Task Consume(ConsumeContext<WebhookTriggered> context)
        {
            await Task.Delay(1 * 1000 * 10);
            await Console.Out.WriteLineAsync($"subscruption: {context.Message.SubscriptionId}");
            using var httpClient = httpClientFactory.CreateClient();
            var payload = new WebhookPayload
            {
                Id = Guid.NewGuid(),
                EventType = context.Message.EventType,
                SubsriptionId = context.Message.SubscriptionId,
                Timestamp = DateTime.Now,
                Data = context.Message.Data
            };

            var jsonPayload = JsonSerializer.Serialize(payload);

            try
            {
                var response = await httpClient.PostAsJsonAsync(context.Message.WebhookUrl, payload);
                response.EnsureSuccessStatusCode();

                var attempt = new WebhookDeliveryAttempt
                {
                    Id = Guid.NewGuid(),
                    WebhookSubscriptionId = context.Message.SubscriptionId,
                    Payload = jsonPayload,
                    ResponseStatusCode = (int)response.StatusCode,
                    Success = response.IsSuccessStatusCode,
                    TimeStamp = DateTime.UtcNow
                };

                await dbContext.WebhookDeliveryAttempts.AddAsync(attempt);

                await dbContext.SaveChangesAsync();
            }
            catch (Exception)
            {

                var attempt = new WebhookDeliveryAttempt
                {
                    Id = Guid.NewGuid(),
                    WebhookSubscriptionId = context.Message.SubscriptionId,
                    Payload = jsonPayload,
                    ResponseStatusCode = null,
                    Success = false,
                    TimeStamp = DateTime.UtcNow
                };

                await dbContext.WebhookDeliveryAttempts.AddAsync(attempt);

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
