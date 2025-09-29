using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Customer.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Customer.Infrastructure.Api.External;

[ApiController]
[Route("api/customer/external/[controller]")]
[ApiExplorerSettings(GroupName = "Customer.External")]
public class CustomersController : ControllerBase
{
    private readonly CustomerDbContext _db;

    public CustomersController(CustomerDbContext db)
    {
        _db = db;
    }

    // POST api/customer/external/customers
    [HttpPost]
    public async Task<IActionResult> Create([FromQuery] string email, CancellationToken ct)
    {
        var customer = new Customer { Id = Guid.NewGuid(), Email = email };

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        await _db.Customers.AddAsync(customer, ct);
        await _db.SaveChangesAsync(ct);

        var evt = new CustomerRegistered(customer.Id, customer.Email);
        var payload = JsonSerializer.Serialize(evt);
        _db.Outbox.Add(new OutboxMessage { Type = nameof(CustomerRegistered), Payload = payload });
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return Created($"/api/customer/external/customers/{customer.Id}", customer);
    }
}
