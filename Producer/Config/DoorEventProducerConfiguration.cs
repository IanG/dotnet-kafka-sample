namespace DotNetKafkaSample.Producer.Config;

public class DoorEventProducerConfiguration
{
    public required string BootstrapServers { get; init; }
    public required string ClientId { get; init; }
    public required string Topic { get; init; }
    public required int PauseAfterSendMs { get; init; }
}