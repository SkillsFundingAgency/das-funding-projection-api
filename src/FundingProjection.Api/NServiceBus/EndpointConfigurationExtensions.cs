extern alias AzureIdentity;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using NServiceBus;
using NServiceBus.Container;
using NServiceBus.ObjectBuilder.MSDependencyInjection;

namespace SFA.DAS.FundingProjection.Api.NServiceBus;

[ExcludeFromCodeCoverage]
public static class EndpointConfigurationExtensions
{
    extension(EndpointConfiguration endpointConfiguration)
    {
        public EndpointConfiguration UseLicense(string? license)
        {
            if (!string.IsNullOrEmpty(license))
            {
                endpointConfiguration.License(WebUtility.HtmlDecode(license));
            }

            return endpointConfiguration;
        }

        public EndpointConfiguration UseConnectionString(string? connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                var learningTransportDirectory = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, ".learningtransport");
                if (!Directory.Exists(learningTransportDirectory))
                {
                    Directory.CreateDirectory(learningTransportDirectory);
                }
                endpointConfiguration.UseTransport<LearningTransport>().StorageDirectory(learningTransportDirectory);
            }
            else
            {
                endpointConfiguration.UseAzureServiceBusTransport(connectionString);
            }

            return endpointConfiguration;
        }

        public EndpointConfiguration UseErrorQueue(string errorQueue)
        {
            endpointConfiguration.SendFailedMessagesTo(errorQueue);
            return endpointConfiguration;
        }

        public EndpointConfiguration UseInstallers()
        {
            endpointConfiguration.EnableInstallers();
            return endpointConfiguration;
        }
    }

    private static Func<Type, bool> CommandsConvention => t => (t.Namespace?.EndsWith("Commands") ?? false) || t.GetInterface("ICommand") is not null;
    private static Func<Type, bool> EventsConvention => t => (t.Namespace?.EndsWith("Events") ?? false) || t.GetInterface("IEvent") is not null;
    
    extension(EndpointConfiguration config)
    {
        public EndpointConfiguration UseMessageConventions()
        {
            var conventionsBuilder = config.Conventions();
            conventionsBuilder.DefiningCommandsAs(CommandsConvention);
            conventionsBuilder.DefiningEventsAs(EventsConvention);
            return config;
        }

        public EndpointConfiguration UseServicesBuilder(UpdateableServiceProvider serviceProvider)
        {
            config.UseContainer<ServicesBuilder>((Action<ContainerCustomizations>) (c => c.ServiceProviderFactory((Func<IServiceCollection, UpdateableServiceProvider>) (_ => serviceProvider))));
            return config;
        }

        public EndpointConfiguration UseNewtonsoftJsonSerializer()
        {
            config.UseSerialization<NewtonsoftJsonSerializer>();
            return config;
        }

        private EndpointConfiguration UseAzureServiceBusTransport(string connectionString, Action<RoutingSettings>? routing = null)
        {
            var transportExtensions = config.UseTransport<AzureServiceBusTransport>();
            transportExtensions.CustomTokenCredential(new AzureIdentity::Azure.Identity.DefaultAzureCredential());
            transportExtensions.ConnectionString(connectionString.Replace("Endpoint=sb://", string.Empty).TrimEnd('/'));
            transportExtensions.Transactions(TransportTransactionMode.ReceiveOnly);
            transportExtensions.SubscriptionRuleNamingConvention(RuleNameShortener.Shorten);
            routing?.Invoke(transportExtensions.Routing());
            return config;
        }
    }
}
