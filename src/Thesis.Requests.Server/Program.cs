using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Seljmov.AspNet.Commons.Helpers;
using Thesis.Requests.Server;
using Thesis.Requests.Server.Options;
using Thesis.Requests.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(nameof(RabbitMqOptions)));
builder.Services.AddScoped<OutgoingRabbitService>();
builder.Services.AddSingleton<IncomingRabbitService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddDbContext<DatabaseContext>(options => options.UseNpgsql(connectionString));

var app = builder.BuildWebApplication();
using var scope = app.Services.CreateScope();
RunRabbitService(scope);

app.Run();

static void RunRabbitService(IServiceScope scope)
{
    var service = scope.ServiceProvider.GetRequiredService<IncomingRabbitService>();
    service.Fetch();
}