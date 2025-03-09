namespace Webhook.Models
{
    public class WebhookPayload<T>()
    {
        public Guid Id { get; set; }
        public string EventType { get; set; }
        public Guid SubsriptionId { get; set; }
        public DateTime Timestamp { get; set; }
        public T Data { get; set; }
    }
}
