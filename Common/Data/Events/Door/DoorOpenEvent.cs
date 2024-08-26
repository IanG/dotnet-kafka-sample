namespace DotNetKafkaSample.Common.Data.Events.Door;

public class DoorOpenEvent : DoorEvent
{
    public override string EventType => "DoorOpen";
}