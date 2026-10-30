using FluentValidation;

namespace SFA.DAS.FundingProjection.Api.AppStart;

public static class ValidationServiceCollectionExtensions
{
    public static IServiceCollection AddFluentValidators(this IServiceCollection services)
    {
        // Register validators from assembly
        services.AddValidatorsFromAssembly(typeof(Program).Assembly, includeInternalTypes: true);

        return services;
    }
}