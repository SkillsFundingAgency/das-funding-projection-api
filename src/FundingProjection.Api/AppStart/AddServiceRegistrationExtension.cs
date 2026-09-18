using Microsoft.EntityFrameworkCore;
using SFA.DAS.FundingProjection.Data;
using SFA.DAS.FundingProjection.Data.Repositories;
using SFA.DAS.FundingProjection.Domain.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.FundingProjection.Api.AppStart;

[ExcludeFromCodeCoverage]
public static class AddServiceRegistrationExtension
{
    public static void AddApplicationDependencies(this IServiceCollection services)
    {
        // validators
        services.AddSingleton(TimeProvider.System);
        services.AddFluentValidators();
        services.AddDistributedMemoryCache();
    }

    public static void AddDatabaseRegistration(this IServiceCollection services,
        ConnectionStrings config,
        string? environmentName)
    {
        services.AddHttpContextAccessor();

        if (string.Equals(environmentName, "DEV", StringComparison.CurrentCultureIgnoreCase))
        {
            services.AddDbContext<FundingProjectionDataContext>(options =>
                options.UseInMemoryDatabase("SFA.DAS.FundingProjection.Api"), ServiceLifetime.Transient);
        }
        else
        {
            services.AddDbContext<FundingProjectionDataContext>(options =>
                options.UseSqlServer(config.SqlConnectionString), ServiceLifetime.Transient);
        }

        services.AddScoped<IFundingProjectionDataContext, FundingProjectionDataContext>(provider =>
            provider.GetRequiredService<FundingProjectionDataContext>());
        services.AddScoped(provider =>
            new Lazy<FundingProjectionDataContext>(provider.GetRequiredService<FundingProjectionDataContext>));

        services.AddScoped<IEmployerFundingProjectionRepository, EmployerFundingProjectionRepository>();
        services.AddScoped<ICommittedLearnerRepository, CommittedLearnerRepository>();
        services.AddScoped<IImportJobStateRepository, ImportJobStateRepository>();
    }

    public static void ConfigureHealthChecks(this IServiceCollection services)
    {
        // health checks
        services
            .AddHealthChecks()
            .AddCheck<DefaultHealthCheck>("default");
    }
}