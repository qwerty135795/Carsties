using System.Security.Claims;

namespace AuctionService.UnitTests;
public static class Helper
{
    public static ClaimsPrincipal GetIdentity()
    {
        var claims = new List<Claim> {new Claim(ClaimTypes.Name, "test")};
        var identity = new ClaimsIdentity(claims);
        return new ClaimsPrincipal(identity);
    }
}
