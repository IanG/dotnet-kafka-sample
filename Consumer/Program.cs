using DotNetKafkaSample.Consumer.Workers;
using Serilog;
using Serilog.Core;
using TestKafkaMessageProducer.Config;

namespace DotNetKafkaSample.Consumer;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        
        Logger logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();

        builder.Services.AddSerilog(logger);
        
        builder.Services.Configure<DoorEventConsumerWorkerConfiguration>(
            builder.Configuration.GetSection("DoorEventConsumerWorker"));
        
        builder.Services.AddHostedService<DoorEventConsumer>();
        
        var host = builder.Build();
        host.Run();
    }
}