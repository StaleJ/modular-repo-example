using System;
using System.Threading;
using System.Threading.Tasks;

namespace Customer.Contracts;

public interface ICustomerApi
{
    Task<bool> ExistsAsync(Guid customerId, CancellationToken ct = default);
}
