using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Threading.Channels;
using Webhook.Data;
using Webhook.Models;
using Webhook.Services;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddHttpClient();
builder.Services.AddScoped<WebhookDispatcher>();

builder.Services.AddDbContext<WebhooksDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("webhooks")));

//builder.Services.AddHostedService<WebhookProcessor>();

//builder.Services.AddSingleton(_ =>
//{
//    return Channel.CreateBounded<WebhookDispatch>(new BoundedChannelOptions(100) { FullMode = BoundedChannelFullMode.Wait });
//});

builder.Services.AddMassTransit(busConfig =>
{
    busConfig.SetKebabCaseEndpointNameFormatter();

    busConfig.AddConsumer<WebhookDispatchedConsumer>();

    busConfig.AddConsumer<WebhookTriggeredConsumer>();

    busConfig.UsingRabbitMq((context, config) =>
    {

        var rabbitmq = builder.Configuration.GetConnectionString("rabbitmq");
        config.Host(rabbitmq);

        config.ConfigureEndpoints(context);
    });
});


var app = builder.Build();

app.UseHttpsRedirection();

app.MapPost("webhook/subscriptions", async (CreateWebhookRequest request, WebhooksDbContext context) =>
{
    var subscription = new WebhookSubscription(Guid.NewGuid(), request.EventType, request.WebhookUrl, DateTime.UtcNow);

    context.WebhookSubscriptions.Add(subscription);

    await context.SaveChangesAsync();

    return Results.Ok(subscription);


});

app.MapPost("/orders", async (CreateOrderRequest request, WebhooksDbContext context, WebhookDispatcher webhookDispatcher) =>
{
    var order = new Order(Guid.NewGuid(), request.CustomerName, request.Amount, DateTime.UtcNow);

    context.Orders.Add(order);

    await context.SaveChangesAsync();

    await webhookDispatcher.DispatchAsync("order.created", order);

    return Results.Ok(order);

}).WithTags("Orders");


app.MapGet("/orders", async (WebhooksDbContext context) =>
{
    return Results.Ok(await context.Orders.ToListAsync());
}).WithTags("Orders");

app.Run();

