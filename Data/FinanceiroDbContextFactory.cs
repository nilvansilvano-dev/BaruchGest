using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FinanceiroAPI.Data;

public class FinanceiroDbContextFactory : IDesignTimeDbContextFactory<FinanceiroDbContext>
{
    public FinanceiroDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var options = new DbContextOptionsBuilder<FinanceiroDbContext>()
            .UseSqlServer(config.GetConnectionString("SqlServer"))
            .Options;

        return new FinanceiroDbContext(options);
    }
}
