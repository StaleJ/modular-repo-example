using Microsoft.AspNetCore.Mvc;

namespace LoanApplication.Infrastructure.Api.Internal;

[ApiController]
[Route("api/loanapp/internal/[controller]")]
[ApiExplorerSettings(GroupName = "LoanApplication.Internal")]
public class LoanAppsAdminController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping() => Ok("loanapp-internal-ok");
}
