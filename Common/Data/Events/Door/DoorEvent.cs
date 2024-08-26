using System.Text.Json.Serialization;
using DotNetKafkaSample.Common.Data.Events.Door.Converters.Json;

namespace DotNetKafkaSample.Common.Data.Events.Door;

[JsonConverter(typeof(DoorEventJsonConverter))]
public abstract class DoorEvent
{ 
    public Guid EventId { get; init; } = Guid.NewGuid(); 
    public DateTime EventDateTime { get; init; } = DateTime.UtcNow;
    public abstract string EventType { get; }
    public required string DoorId { get; init; }
}