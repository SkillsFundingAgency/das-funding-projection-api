using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.FundingProjection.Api;

[ExcludeFromCodeCoverage]
public class Program
{
    public static void Main(string[] args)
    {
        // This is to aid the document generation
        if (Environment.GetEnvironmentVariable("_MSBUILDTLENABLED") is not null)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "http://localhost:0");
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<DesignTimeStartup>())
                .Build()
                .Run();
            return;
        }

        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }

    private sealed class DesignTimeStartup(IConfiguration configuration)
    {
        private readonly Startup _inner = new(configuration);
        public void ConfigureServices(IServiceCollection services) => _inner.ConfigureServices(services);
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) => _inner.Configure(app, env);
    }
}