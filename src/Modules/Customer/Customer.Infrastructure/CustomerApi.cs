using System;
using System.Threading;
using System.Threading.Tasks;
using Customer.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Customer.Infrastructure;

internal class CustomerApi : ICustomerApi
{
    private readonly CustomerDbContext _db;
    public CustomerApi(CustomerDbContext db) => _db = db;

    public Task<bool> ExistsAsync(Guid customerId, CancellationToken ct = default)
        => _db.Customers.AnyAsync(x => x.Id == customerId, ct);
}

public static class CustomerModule
{
    public static IServiceCollection AddCustomerModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<CustomerDbContext>(o =>
            o.UseSqlite(connectionString,
                b => b.MigrationsAssembly("Customer.Infrastructure")));

        services.TryAddScoped<ICustomerApi, CustomerApi>();
        return services;
    }
}
