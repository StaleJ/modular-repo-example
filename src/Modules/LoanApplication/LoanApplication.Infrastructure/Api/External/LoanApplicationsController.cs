using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Customer.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace LoanApplication.Infrastructure.Api.External;

[ApiController]
[Route("api/loanapp/external/[controller]")]
[ApiExplorerSettings(GroupName = "LoanApplication.External")]
public class LoanApplicationsController : ControllerBase
{
    private readonly LoanAppDbContext _db;
    private readonly ICustomerApi _customers;

    public LoanApplicationsController(LoanAppDbContext db, ICustomerApi customers)
    {
        _db = db;
        _customers = customers;
    }

    // POST api/loanapp/external/loanapplications
    [HttpPost]
    public async Task<IActionResult> Create([FromQuery] Guid customerId, [FromQuery] decimal amount, CancellationToken ct)
    {
        if (!await _customers.ExistsAsync(customerId, ct))
        {
            return BadRequest("Customer does not exist.");
        }

        var appEntity = new LoanApp
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Amount = amount,
            Status = "Submitted"
        };

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        await _db.LoanApplications.AddAsync(appEntity, ct);
        await _db.SaveChangesAsync(ct);

        var payload = JsonSerializer.Serialize(new { appEntity.Id, appEntity.CustomerId, appEntity.Amount });
        _db.Outbox.Add(new OutboxMessage
        {
            Type = "LoanApplicationSubmitted",
            Payload = payload
        });
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return Created($"/api/loanapp/external/loanapplications/{appEntity.Id}", appEntity);
    }
}
