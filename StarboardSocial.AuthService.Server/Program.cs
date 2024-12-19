using System.Text.Json.Serialization;
using RabbitMQ.Client;
using StarboardSocial.AuthService.Domain.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// RabbitMQ Config
try
{
    ConnectionFactory factory = new()
    {
        UserName = builder.Configuration["Rabbit:UserName"]!,
        Password = builder.Configuration["Rabbit:Password"]!,
        VirtualHost = builder.Configuration["Rabbit:VirtualHost"]!,
        HostName = builder.Configuration["Rabbit:HostName"]!,
        Port = int.Parse(builder.Configuration["Rabbit:Port"]!)
    };

    IConnection conn = await factory.CreateConnectionAsync();
    IChannel channel = await conn.CreateChannelAsync();

    builder.Services.AddSingleton(channel);
} catch (Exception e)
{
    Console.WriteLine("Error connecting to RabbitMQ");
    Console.WriteLine(e.Message);
}

builder.Services.AddTransient<ITokenService, TokenService>();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UsePathBase("/auth");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();