using ITHelpdesk.API.Hubs;
using ITHelpdesk.API.Services;
using ITHelpdesk.Application.DependencyInjection;
using ITHelpdesk.Application.Interfaces.Services;
using ITHelpdesk.Infrastructure.DependencyInjection;
using ITHelpdesk.Infrastructure.Identity;
using ITHelpdesk.Infrastructure.Persistence;
using ITHelpdesk.Infrastructure.Persistence.Seeders;
using Microsoft.AspNetCore.Identity;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddSignalR();
builder.Services.AddScoped<
    INotificationPublisher,
    SignalRNotificationPublisher>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    var dbContext = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    await RoleSeeder.SeedAsync(roleManager);

    await AdminUserSeeder.SeedAsync(
        app.Services,
        app.Configuration);

    await SlaPolicySeeder.SeedAsync(dbContext);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
