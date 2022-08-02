using Messaging;
using Microsoft.Extensions.Options;
using OrderService;
using RabbitMQ.Client;
using Steeltoe.Messaging.RabbitMQ.Core;
using Steeltoe.Messaging.RabbitMQ.Extensions;
using System.Text;
using System.Text.Json;

public class RabbitSender
{
	private readonly RabbitMQSettings _rabbitSettings;
    private readonly RabbitTemplate _template;
    public RabbitSender(RabbitMQSettings rabbitSettings,
        IServiceProvider services)
	{
		
		_rabbitSettings = rabbitSettings;
        _template = services.GetRabbitTemplate();	

    }
	
	public void PublishMessage<T>(T entity, string key) where T : class
	{
		var message = JsonSerializer.Serialize(entity);
		//topic should become enum or similar
		var body = Encoding.UTF8.GetBytes(message);
        _template.ConvertAndSend(exchange: _rabbitSettings.ExchangeName,
									 routingKey: key,
									 message: body);
		Console.WriteLine(" [x] Sent '{0}':'{1}'", key, message);

	}
}