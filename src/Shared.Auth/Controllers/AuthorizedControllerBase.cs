using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Shared.Auth.Controllers
{
    [ApiController]
    [Authorize(Roles = "clear-budget")]
    public abstract class AuthorizedControllerBase : ControllerBase
    {
        protected Guid UserId
        {
            get
            {
                var idClaim = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
                if (Guid.TryParse(idClaim, out var userId))
                {
                    return userId;
                }

                throw new UnauthorizedAccessException("Invalid or missing user ID in token.");
            }
        }

    }
}
