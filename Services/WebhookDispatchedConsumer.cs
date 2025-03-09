using MassTransit;
using Microsoft.EntityFrameworkCore;
using Webhook.Data;
using Webhook.Models;

namespace Webhook.Services
{
    internal sealed class WebhookDispatchedConsumer(WebhooksDbContext dbContext) : IConsumer<WebhookDispatch>
    {
        public async Task Consume(ConsumeContext<WebhookDispatch> context)
        {
            var message = context.Message;
            var subscriptions = await dbContext.WebhookSubscriptions.AsNoTracking().Where(s => s.EventType == message.eventType).ToListAsync();

            foreach (var subscription in subscriptions)
            {
                await context.Publish(new WebhookTriggered(
                    subscription.Id, subscription.EventType, subscription.WebhookUrl, message.Data));
            }

        }
    }
}

