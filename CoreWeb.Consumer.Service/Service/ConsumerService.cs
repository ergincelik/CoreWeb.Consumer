using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CoreWeb.Consumer.Common;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Hosting;
using System.Threading;

namespace CoreWeb.Consumer.Service.Service
{
    public class ConsumerService : BackgroundService
    {
        private readonly ILogger<ConsumerService> _logger;
        private readonly RabbitMqConfiguration _rabbitMqConfiguration;


        // rabbit mq
        private IConnection _connection;
        private IModel _channel;

        public ConsumerService(RabbitMqConfiguration rabbitMqConfiguration, ILogger<ConsumerService> logger)
        {
            _logger = logger;
            _rabbitMqConfiguration = rabbitMqConfiguration;
            InitRabbitMQ();
        }
         
        private void InitRabbitMQ()
        {
            // create factory
            var factory = new ConnectionFactory
            {
                HostName = _rabbitMqConfiguration.HostName,
                UserName = _rabbitMqConfiguration.UserName,
                Password = _rabbitMqConfiguration.Password
            };


            // create connection
            _connection = factory.CreateConnection();

            // create channel  
            _channel = _connection.CreateModel();

            // declare exchange
            _channel.ExchangeDeclare(_rabbitMqConfiguration.ExchangeName, ExchangeType.Direct, true, false, null);

            // declare queue
            _channel.QueueDeclare(_rabbitMqConfiguration.QueueName, true, false, false, null);

            // bind queue
            _channel.QueueBind(_rabbitMqConfiguration.QueueName, _rabbitMqConfiguration.ExchangeName, string.Empty, null);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();


            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                // received message  
                var content = Encoding.UTF8.GetString(ea.Body.Span);

                // handle the received message  
                _logger.LogInformation(content);
                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(_rabbitMqConfiguration.QueueName, false, consumer);

            await Task.Delay(1000, stoppingToken);
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
