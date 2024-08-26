using DotNetKafkaSample.Producer.Config;
using DotNetKafkaSample.Producer.Workers;
using Serilog;
using Serilog.Core;

namespace DotNetKafkaSample.Producer;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        Logger logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();
        
        builder.Services.AddSerilog(logger);

        builder.Services.Configure<DoorEventProducerConfiguration>(
            builder.Configuration.GetSection("DoorEventProducerWorker"));
        
        builder.Services.AddHostedService<DoorEventProducer>();
        
        var host = builder.Build();
        host.Run();
    }
}