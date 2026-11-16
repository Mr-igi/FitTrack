using FitTrack.Api.Data;
using FitTrack.Core.Repositories;
using FitTrack.Core.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddSingleton(typeof(IRepository<>), typeof(InMemoryRepository<>));

builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<MembershipService>();
builder.Services.AddScoped<StatisticsService>();

var app = builder.Build();

SeedData.Seed(app.Services);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();