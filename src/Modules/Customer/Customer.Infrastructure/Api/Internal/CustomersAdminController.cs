using Microsoft.AspNetCore.Mvc;

namespace Customer.Infrastructure.Api.Internal;

[ApiController]
[Route("api/customer/internal/[controller]")]
[ApiExplorerSettings(GroupName = "Customer.Internal")]
public class CustomersAdminController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping() => Ok("customer-internal-ok");
}
