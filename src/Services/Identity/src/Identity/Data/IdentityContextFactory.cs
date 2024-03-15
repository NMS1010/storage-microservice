using BuildingBlocks.Constants;
using BuildingBlocks.EFCore;
using BuildingBlocks.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Identity.Data
{
    public class IdentityContextFactory : IDesignTimeDbContextFactory<IdentityContext>
    {
        public IdentityContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{AppConstants.ASPNETCORE_ENVIRONMENT}.json", optional: true, reloadOnChange: true)
                .Build();

            var postgresOptions = configuration.GetOptions<PostgresOptions>(nameof(PostgresOptions));

            var optionBuilder = new DbContextOptionsBuilder<IdentityContext>();
            optionBuilder.UseNpgsql(postgresOptions.ConnectionString)
                .UseSnakeCaseNamingConvention(); ;

            return new IdentityContext(optionBuilder.Options);
        }
    }
}
