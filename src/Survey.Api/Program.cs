using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Survey.Infrastructure.Context;
using Survey.Infrastructure.Interfaces;
using Survey.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddDbContext<SurveyDbContext>(options =>
{
    var connectionString = builder.Configuration["Survey:ConnectionString"];
    options.UseNpgsql(connectionString);
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();