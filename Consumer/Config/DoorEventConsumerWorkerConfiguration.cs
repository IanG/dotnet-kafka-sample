namespace TestKafkaMessageProducer.Config;

public class DoorEventConsumerWorkerConfiguration
{
    public required string BootstrapServers { get; init; }
    public required string ClientId { get; init; }
    public required string GroupId { get; init; }
    public required string Topic { get; init; }
}