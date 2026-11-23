using ITHelpdesk.Application.Interfaces.Services;
using ITHelpdesk.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ITHelpdesk.Application.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<ITicketCategoryService, TicketCategoryService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ISlaMonitoringService, SlaMonitoringService>();
        services.AddScoped<ISlaPolicyService, SlaPolicyService>();

        return services;
    }

}