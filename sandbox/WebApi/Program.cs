using WebApi;
using WebApi.Data;
using WebApi.Support;

var builder = WebApplication.CreateBuilder(args);

builder.UseSerilogTwoStageInitialization();
builder.Services.AddControllers();
builder.Services.AddNandelKafka(builder.Configuration.GetSection("Kafka"));
builder.Services.AddNandelKafkaOutbox<WebApiDbContext>();
builder.Services.AddServices();
builder.Services.AddConsumers();
builder.Services.AddData(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{ 
    app.MapOpenApi();
}

app.MapControllers();
app.Run();

