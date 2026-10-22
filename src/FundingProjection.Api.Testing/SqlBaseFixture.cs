using AutoFixture;
using FundingProjection.Api.Testing.DataCreation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.FundingProjection.Data;

namespace FundingProjection.Api.Testing;

public abstract class SqlBaseFixture
{
    // private
    private SqlWebApplication _server;
    private IServiceScope _serviceScope;
    private FundingProjectionDataContext _dataContext;

    // protected
    protected IFixture Fixture;
    protected HttpClient Client;
    public SqlTestDataManager DbData { get; private set; }

    [SetUp]
    public virtual void Setup()
    {
        _server = new SqlWebApplication();
        Client = _server.CreateClient();
        Fixture = new Fixture();
        Fixture.Customizations.Add(new CustomDateTimeSpecimenBuilder());
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _serviceScope = _server.Services.CreateScope();
        _dataContext = _serviceScope.ServiceProvider.GetRequiredService<FundingProjectionDataContext>();
        
        var configuration = _server.Server.Services.GetService(typeof(IConfiguration)) as IConfiguration;
        DbData = new SqlTestDataManager(_dataContext, Fixture, configuration.GetDbSchemaName());
    }

    [TearDown]
    public virtual void Teardown()
    {
        DbData = null!;
        _dataContext.Dispose();
        _serviceScope.Dispose();
        
        Client.Dispose();
        _server.Dispose();
    }
}