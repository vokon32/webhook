namespace Webhook.Models
{
    internal sealed record WebhookDispatch(string eventType, object Data);
}
