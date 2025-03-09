namespace Webhook.Models
{
    public class WebhookPayload()
    {
        public Guid Id { get; set; }
        public string EventType { get; set; }
        public Guid SubsriptionId { get; set; }
        public DateTime Timestamp { get; set; }
        public object Data { get; set; }
    }
}
