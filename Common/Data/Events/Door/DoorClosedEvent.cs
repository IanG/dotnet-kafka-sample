namespace DotNetKafkaSample.Common.Data.Events.Door;

public class DoorClosedEvent : DoorEvent
{
    public override string EventType => "DoorClosed";
}