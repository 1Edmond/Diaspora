using System.Reflection;
using Wallet.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();


builder.Services.AddControllers();
builder.Services.AddOpenApi();


await builder.Services.AddInfrastructureServices(builder.Configuration, Assembly.GetExecutingAssembly());
builder.Host.BuildInfrastructure(
    configuration: builder.Configuration,
    serviceVersion: "1.0.0"
    );


var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await app.UseInfrastructure();
_ = app.PublishWalletResources<Program>();

app.MapControllers();


app.Run();
