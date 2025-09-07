using Serilog;
using ThroughputBenchmark;

var builder = WebApplication.CreateBuilder(args);

builder.UseSerilogTwoStageInitialization();

builder.Services.AddNandelKafka(builder.Configuration.GetSection("Connection"));
builder.Services.AddSingleton<Bench>();
builder.Services.AddSingleton<BenchState>();
builder.Services.AddSingleton<Producer>();
builder.Services.AddMessageConsumer<Message, Consumer>();

var app = builder.Build();

const int testWindowSeconds = 120;

var cts = new CancellationTokenSource(delay: TimeSpan.FromMinutes(15));
var bench = app.Services.GetRequiredService<Bench>();

await app.StartAsync(cts.Token);
await bench.ExecuteAsync(testWindowSeconds, cts.Token)
    .ContinueWith(task =>
    {
        var result = task.GetAwaiter().GetResult();
        foreach (var key in result.AllKeys)
        {
            Console.WriteLine($"{key}: {result[key]}");
        }
    });
await app.StopAsync(CancellationToken.None);