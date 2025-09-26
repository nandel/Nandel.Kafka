using Confluent.Kafka;
using Confluent.Kafka.Admin;
using ThroughputBenchmark.Benchmark;
using ThroughputBenchmark.Components;
using ThroughputBenchmark.Support;

// Settings
const int benchmarkTimeSeconds = 60 * 5;
const int benchmarkTimeoutSeconds = benchmarkTimeSeconds * 2;
const int benchmarkDelaySeconds = 15;

// Benchmark Run Script
var builder = WebApplication.CreateBuilder(args);

builder.UseSerilogTwoStageInitialization();

builder.Services.AddNandelKafka(builder.Configuration.GetSection("Connection"));
builder.Services.AddSingleton<Benchmark>();
builder.Services.AddSingleton<BenchmarkState>();
builder.Services.AddSingleton<Producer>();
builder.Services.AddKafkaMessageConsumer<MessageBody, Consumer>();

var app = builder.Build();
var cts = new CancellationTokenSource(delay: TimeSpan.FromSeconds(benchmarkTimeoutSeconds));

var benchmark = app.Services.GetRequiredService<Benchmark>();
var admin = app.Services.GetRequiredService<IAdminClient>();

try
{
    await admin.DeleteTopicsAsync([MessageBody.TopicName]);
}
catch (Exception e) when (e is DeleteTopicsException)
{
    // We need to ensure that is deleted before we start
    // So if it already does not exist, it's ok
}

await app.StartAsync(cts.Token);
await Task.Delay(TimeSpan.FromSeconds(benchmarkDelaySeconds), cts.Token);
await benchmark.ExecuteAsync(benchmarkTimeSeconds, cts.Token)
    .ContinueWith(task =>
    {
        var result = task.GetAwaiter().GetResult();
        foreach (var key in result.AllKeys)
        {
            Console.WriteLine($"{key}: {result[key]}");
        }
    });
await app.StopAsync(CancellationToken.None);