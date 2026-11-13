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
    protected HttpClient Client;
    
    // public
    public SqlTestDataManager DbData { get; private set; }

    [SetUp]
    public virtual void Setup()
    {
        _server = new SqlWebApplication();
        Client = _server.CreateClient();
        _serviceScope = _server.Services.CreateScope();
        _dataContext = _serviceScope.ServiceProvider.GetRequiredService<FundingProjectionDataContext>();
        DbData = new SqlTestDataManager(_dataContext);
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