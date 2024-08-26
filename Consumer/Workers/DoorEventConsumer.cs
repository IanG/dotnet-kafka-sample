using System.Text.Json;
using Confluent.Kafka;
using DotNetKafkaSample.Common.Data.Events.Door;
using Microsoft.Extensions.Options;
using TestKafkaMessageProducer.Config;

namespace DotNetKafkaSample.Consumer.Workers;

public class DoorEventConsumer : BackgroundService
{
    private readonly DoorEventConsumerWorkerConfiguration _configuration;
    private readonly ILogger<DoorEventConsumer> _logger;
    
    private static readonly JsonSerializerOptions SerializerOptions = new ()
    {
        PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower
    };

    public DoorEventConsumer(IOptions<DoorEventConsumerWorkerConfiguration> configuration,
        ILogger<DoorEventConsumer> logger)
    {
        _configuration = configuration.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using IConsumer<string, string>? consumer = new ConsumerBuilder<string, string>(GetConsumerConfig()).Build();
        
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Consuming DoorEvent messages from {bootstrapServers} topic {topic} as client {clientId} for group {clientGroupId}",
                _configuration.BootstrapServers, _configuration.Topic, _configuration.ClientId, _configuration.GroupId);
        }
        
        consumer.Subscribe(_configuration.Topic);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Waiting for next message in topic {topic}", _configuration.Topic);
                }
                
                ConsumeResult<string, string> consumeResult = consumer.Consume(stoppingToken);

                if (consumeResult is not null)
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug("Consuming {messageId} from partition {partition} Offset {offset} Message {message}", 
                            consumeResult.Message.Key, consumeResult.Partition.Value, consumeResult.Offset.Value, consumeResult.Message.Value);
                    }

                    DoorEvent? doorEvent = MessageToDoorEvent(consumeResult.Message.Value);

                    if (doorEvent is not null)
                    {
                        if (_logger.IsEnabled(LogLevel.Debug))
                        {
                            _logger.LogDebug("Consuming event id {eventId} type {eventType} for door {doorId}", doorEvent.EventId, doorEvent.GetType().Name, doorEvent.DoorId);
                        }
                    }

                    consumer.Commit(consumeResult);
                }
            }
            catch (ConsumeException e)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError("Kafka consume error {kafkaError}", e);
                }
            }
        }
        
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Stopping Message Consumer with {name}", consumer.Name);
        }

        consumer.Unsubscribe();
        consumer.Close();
    }

    private DoorEvent? MessageToDoorEvent(string message)
    {
        return JsonSerializer.Deserialize<DoorEvent>(message, SerializerOptions);
    }
    
    private ConsumerConfig GetConsumerConfig()
    {
        return new ConsumerConfig()
        {
            BootstrapServers = _configuration.BootstrapServers,
            ClientId = _configuration.ClientId,
            GroupId = _configuration.GroupId,
        };
    }
}