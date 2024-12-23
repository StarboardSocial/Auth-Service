using System.IdentityModel.Tokens.Jwt;

namespace StarboardSocial.UserService.Server.Helpers;

public static class UserIdHelper
{
    public static string GetUserId(HttpRequest httpRequest)
    {
        string token = httpRequest.Headers.Authorization.ToString();
        if (!token.StartsWith("Bearer")) throw new UnauthorizedAccessException("No bearer token was found");
        
        string[] tokenArray = token.Split(" ");
        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken? jwtSecurityToken = handler.ReadJwtToken(tokenArray[1]);

        return jwtSecurityToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value ?? throw new UnauthorizedAccessException("No sub claim was found");
    }
}