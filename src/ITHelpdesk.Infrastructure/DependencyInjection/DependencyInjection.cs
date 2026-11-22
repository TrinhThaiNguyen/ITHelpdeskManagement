using ITHelpdesk.Application.Common.Interfaces;
using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Application.Interfaces.Services;
using ITHelpdesk.Application.Services;
using ITHelpdesk.Infrastructure.Persistence;
using ITHelpdesk.Infrastructure.Repositories;
using ITHelpdesk.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ITHelpdesk.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


namespace ITHelpdesk.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");


        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found.");
        }

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.Configure<JwtSettings>(
            configuration.GetSection(JwtSettings.SectionName));

        services.AddDataProtection();

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                // Password policy
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;

                // Account policy
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        //
        var jwtSettings = configuration
            .GetSection(JwtSettings.SectionName)
            .Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT settings are missing.");

        if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
        {
            throw new InvalidOperationException("JWT SecretKey is missing.");
        }

        if (Encoding.UTF8.GetByteCount(jwtSettings.SecretKey) < 32)
        {
            throw new InvalidOperationException("JWT SecretKey must be at least 32 bytes.");
        }

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,

                    ValidateLifetime = true,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),

                    ClockSkew = TimeSpan.Zero
                };
            });



        // Register Repositories
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<ITicketCategoryRepository, TicketCategoryRepository>();
        services.AddScoped<ITicketHistoryRepository, TicketHistoryRepository>();
        services.AddScoped<ITicketCommentRepository, TicketCommentRepository>();
        services.AddScoped<ITicketCommentService, TicketCommentService>();
        services.AddScoped<ITicketAttachmentRepository, TicketAttachmentRepository>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<ITicketAttachmentService, TicketAttachmentService>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<ITicketAssignmentService, TicketAssignmentService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}