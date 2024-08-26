using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetKafkaSample.Common.Data.Events.Door.Converters.Json;

public class DoorEventJsonConverter : JsonConverter<DoorEvent>
{
    public override DoorEvent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        using JsonDocument document = JsonDocument.ParseValue(ref reader);
        JsonElement root = document.RootElement;

        if (root.TryGetProperty("event-type", out JsonElement eventTypeElement))
        {
            string? eventType = eventTypeElement.GetString();

            DoorEvent doorEvent = eventType switch
            {
                "DoorOpen" => JsonSerializer.Deserialize<DoorOpenEvent>(root.GetRawText(), options),
                "DoorClosed" => JsonSerializer.Deserialize<DoorClosedEvent>(root.GetRawText(), options),
                _ => throw new JsonException($"Unknown event type: {eventType}")
            };

            return doorEvent!;
        }

        throw new JsonException("Could not create DoorEvent as the event-type property is missing");
    }

    public override void Write(Utf8JsonWriter writer, DoorEvent doorEvent, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, doorEvent, doorEvent.GetType(), options);
    }
}