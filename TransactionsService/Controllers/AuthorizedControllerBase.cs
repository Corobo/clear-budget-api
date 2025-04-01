using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TransactionsService.Controllers
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
