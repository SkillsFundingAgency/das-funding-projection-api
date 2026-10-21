extern alias AzureIdentity;
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.FundingProjection.Data;
using SFA.DAS.FundingProjection.Data.Repositories;
using SFA.DAS.FundingProjection.Domain.Configuration;
using System.Diagnostics.CodeAnalysis;
using SFA.DAS.EmployerFinance.Types.Models;
using SFA.DAS.FundingProjection.Domain.Services;

namespace SFA.DAS.FundingProjection.Api.AppStart;

[ExcludeFromCodeCoverage]
public static class AddServiceRegistrationExtension
{
    extension(IServiceCollection services)
    {
        public void AddApplicationDependencies()
        {
            // validators
            services.AddSingleton(TimeProvider.System);
            services.AddFluentValidators();
            services.AddDistributedMemoryCache();

            // domain services
            services.AddSingleton<IEventService, EventService>();
            services.AddScoped<IExpiredFunds, ExpiredFunds>();
        }

        public void AddServiceBusRegistration(IConfiguration configuration)
        {
            var fqdn = configuration[$"{QueueConstants.ServiceBusConnection}:fullyQualifiedNamespace"];
            var connectionString = configuration[QueueConstants.ServiceBusConnection];

            services.AddSingleton(_ => fqdn != null
                ? new ServiceBusClient(fqdn, new AzureIdentity::Azure.Identity.DefaultAzureCredential())
                : new ServiceBusClient(connectionString));
        }

        public void AddDatabaseRegistration(ConnectionStrings config,
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
            services.AddScoped<IImportJobStateRepository, ImportJobStateRepository>();
            services.AddScoped<IApprenticeshipPaymentSummariesRepository, ApprenticeshipPaymentSummariesRepository>();
        }

        public void ConfigureHealthChecks()
        {
            // health checks
            services
                .AddHealthChecks()
                .AddCheck<DefaultHealthCheck>("default");
        }
    }
}