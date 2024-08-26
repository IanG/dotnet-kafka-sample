using System.Text.Json;
using Confluent.Kafka;
using DotNetKafkaSample.Common.Data.Events.Door;
using DotNetKafkaSample.Producer.Config;
using Microsoft.Extensions.Options;

namespace DotNetKafkaSample.Producer.Workers;

public class DoorEventProducer : BackgroundService
{
    private readonly DoorEventProducerConfiguration _configuration;
    private readonly ILogger<DoorEventProducer> _logger;
    
    private static readonly Random Random = new(420);
    
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower
    };

    public DoorEventProducer(IOptions<DoorEventProducerConfiguration> configuration, ILogger<DoorEventProducer> logger)
    {
        _configuration = configuration.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using IProducer<string, string>? producer = new ProducerBuilder<string, string>(GetProducerConfig()).Build();

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Producing DoorEvent messages to {bootStrapServers} topic {topid} as client {clientId}",
                _configuration.BootstrapServers, _configuration.Topic, _configuration.ClientId);
        }
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                DoorEvent doorEvent = GetNextDoorEvent();
                Message<string, string> message = DoorEventToMessage(doorEvent);
                
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Sending {eventType} event for door {doorId} with id {id} to Kafka...",
                        doorEvent.GetType().Name, doorEvent.DoorId, doorEvent.EventId);
                }
                
                DeliveryResult<string, string> deliveryReport = await producer.ProduceAsync(_configuration.Topic, message, stoppingToken);
                
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Event {id} stored in partition {partition} Offset {offset}", doorEvent.EventId, deliveryReport.TopicPartition.Partition.Value, deliveryReport.Offset.Value);
                }

                int delay = Random.Next(1, 10);
                
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Pausing for {delay}ms", delay);
                }
                
                await Task.Delay(delay, stoppingToken);
            }
            catch (ProduceException<Null, string> e)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError("Kafka error {kafkaError}", e);
                }
                
            }
        }

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Stopping Message Producer with {name}", producer.Name);
        }
        
        producer.Flush(stoppingToken);
    }
    
    private ProducerConfig GetProducerConfig()
    {
        return new ProducerConfig
        {
            BootstrapServers = _configuration.BootstrapServers,
            ClientId = _configuration.ClientId
        };
    }

    private DoorEvent GetNextDoorEvent() => Random.Next(1, 100) <= 50 ? GetDoorOpenEvent() : GetDoorClosedEvent();
    private DoorOpenEvent GetDoorOpenEvent() => new() { DoorId = Random.Next(1, 10).ToString() };
    private DoorClosedEvent GetDoorClosedEvent() => new() { DoorId = Random.Next(1, 10).ToString() };
    
    private Message<string, string> DoorEventToMessage(DoorEvent doorEvent)
    {
        return new Message<string, string>
        {
            Key = doorEvent.EventId.ToString(),
            Value = JsonSerializer.Serialize(doorEvent, SerializerOptions)
        };
    }
}