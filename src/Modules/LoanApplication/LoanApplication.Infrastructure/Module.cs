using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LoanApplication.Infrastructure;

public static class LoanApplicationModule
{
    public static IServiceCollection AddLoanApplicationModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<LoanAppDbContext>(o =>
            o.UseSqlite(connectionString,
                b => b.MigrationsAssembly("LoanApplication.Infrastructure")));
        return services;
    }
}
