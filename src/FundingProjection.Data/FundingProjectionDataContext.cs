using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using SFA.DAS.FundingProjection.Data.Configuration;
using SFA.DAS.FundingProjection.Domain.Configuration;
using SFA.DAS.FundingProjection.Domain.Entities;

namespace SFA.DAS.FundingProjection.Data;

public interface IFundingProjectionDataContext
{
    DbSet<ImportJobStateEntity> ImportJobStates { get; }
    DbSet<CommittedLearnerEntity> CommittedLearners { get; }
    DbSet<CommittedTransferOutEntity> CommittedTransferOuts { get; }
    DbSet<EmployerFundingProjectionEntity> EmployerFundingProjections { get; }
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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FundingProjectionDataContext).Assembly);
        modelBuilder.ApplyConfiguration(new CommittedLearnerEntityConfiguration());
        modelBuilder.ApplyConfiguration(new CommittedTransferOutEntityConfiguration());
        modelBuilder.ApplyConfiguration(new EmployerFundingProjectionEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ImportJobStateConfiguration());
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<ImportJobStateEntity> ImportJobStates { get; set; }
    public DbSet<CommittedLearnerEntity> CommittedLearners { get; set; }
    public DbSet<CommittedTransferOutEntity> CommittedTransferOuts { get; set; }
    public DbSet<EmployerFundingProjectionEntity> EmployerFundingProjections { get; set; }

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