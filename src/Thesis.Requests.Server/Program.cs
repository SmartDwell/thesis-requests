using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Seljmov.AspNet.Commons.Helpers;
using Thesis.Requests.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddDbContext<DatabaseContext>(options => options.UseNpgsql(connectionString));

var app = builder.BuildWebApplication();

app.Run();
