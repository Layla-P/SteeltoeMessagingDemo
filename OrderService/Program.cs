using Messaging;
using Microsoft.AspNetCore.Mvc;
using OrderService;
using Steeltoe.Extensions.Configuration;
using Steeltoe.Messaging.RabbitMQ.Config;
using Steeltoe.Messaging.RabbitMQ.Core;
using Steeltoe.Messaging.RabbitMQ.Extensions;
using System.Xml.Linq;
using static Steeltoe.Messaging.RabbitMQ.Connection.CorrelationData;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>()
    .AddCommandLine(args)
    .Build();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.SetUpRabbitMq(builder.Configuration);
//builder.Services.AddSingleton<RabbitSender>();

var configSection = builder.Configuration.GetSection("RabbitMQSettings");

var settings = new RabbitMQSettings();
configSection.Bind(settings);
builder.Services.AddSingleton<RabbitMQSettings>(settings);
builder.Services.AddRabbitQueue(new Queue(settings.ExchangeName));
builder.Services.AddScoped<RabbitTemplate>(sp=>sp.GetRabbitTemplate());
builder.Services.AddScoped<RabbitSender>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();




var orderIdSeed = 1;
app.MapPost("/waffleOrder", (RabbitSender rabbitSender, [FromBody] Order order) =>
{
    if (order.Id is 0)
    {
        order = new Order().Seed(orderIdSeed);
        orderIdSeed++;
    }
    rabbitSender.PublishMessage<Order>(order, "order.cookwaffle");
});

app.Run();

