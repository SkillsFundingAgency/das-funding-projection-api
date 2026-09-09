using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using SFA.DAS.FundingProjection.Domain.Configuration;

namespace SFA.DAS.FundingProjection.Data;

public interface IFundingProjectionDataContext
{
    DatabaseFacade Database { get; }
    Task Ping(CancellationToken cancellationToken);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    void SetValues<TEntity>(TEntity to, TEntity from) where TEntity : class;
}

public class FundingProjectionDataContext : DbContext, IFundingProjectionDataContext
{
    private readonly ConnectionStrings? _configuration;
    public FundingProjectionDataContext() { }
    public FundingProjectionDataContext(DbContextOptions<FundingProjectionDataContext> options) : base(options) { }
    public FundingProjectionDataContext(IOptions<ConnectionStrings> config, DbContextOptions<FundingProjectionDataContext> options) : base(options)
    {
        _configuration = config.Value;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connection = new SqlConnection { ConnectionString = _configuration!.SqlConnectionString, };
        optionsBuilder.UseSqlServer(connection, options => options.EnableRetryOnFailure(5, TimeSpan.FromSeconds(20), null));
        optionsBuilder.UseLazyLoadingProxies();

        // Note: useful to keep here
        // optionsBuilder.LogTo(message => Debug.WriteLine(message));
        // optionsBuilder.EnableDetailedErrors();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    public async Task Ping(CancellationToken cancellationToken)
    {
        await Database
            .ExecuteSqlRawAsync("SELECT 1;", cancellationToken)
            .ConfigureAwait(false);
    }

    public void SetValues<TEntity>(TEntity to, TEntity from) where TEntity : class
    {
        Entry(to).CurrentValues.SetValues(from);
    }
}