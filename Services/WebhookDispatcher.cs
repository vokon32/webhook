using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Threading.Channels;
using Webhook.Data;
using Webhook.Models;

namespace Webhook.Services
{
    internal sealed class WebhookDispatcher(IPublishEndpoint publishEndpoint)
    {

        public async Task DispatchAsync<T>(string eventType, T data) where T : notnull
        {

            await publishEndpoint.Publish(new WebhookDispatch(eventType, data));
        }
    }


}
