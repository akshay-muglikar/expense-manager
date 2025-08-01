using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace InventoryManagement.Api.Provider;
public class UserServiceProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserServiceProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetUsername()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
    }

    public string? GetClientId()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirst("client_id")?.Value;
    }
}
