using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ReportingService.Controllers
{
    [ApiController]
    [Authorize(Roles = "clear-budget")]
    public abstract class AuthorizedControllerBase : ControllerBase
    {
        protected string UserId =>
            User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("No user ID found in token.");
    }
}
