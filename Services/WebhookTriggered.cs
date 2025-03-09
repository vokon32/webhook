﻿namespace Webhook.Services
{
    internal sealed record WebhookTriggered(Guid SubscriptionId, string EventType, string WebhookUrl, object Data);
}
