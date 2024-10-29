using System.Security.Claims;

namespace AspNetCore_Template.exception;

public class UserByPrincipalNotFoundException : NotFoundException
{
    public UserByPrincipalNotFoundException(ClaimsPrincipal principal) : base($"User with ID: {principal.Identity?.Name} was not found")
    {
    }
}