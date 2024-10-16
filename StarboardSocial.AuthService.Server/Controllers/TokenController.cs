using FluentResults;
using Microsoft.AspNetCore.Mvc;
using StarboardSocial.AuthService.Domain.Models;
using StarboardSocial.AuthService.Domain.Services;

namespace StarboardSocial.AuthService.Server.Controllers;

[ApiController]
[Route("token")]
public class TokenController(ITokenService tokenService): ControllerBase
{
    private readonly ITokenService _tokenService = tokenService;
    
    [HttpPost]
    [Route("exchange")]
    public async Task<IActionResult> ExchangeToken([FromQuery] string code, [FromQuery] string redirectUri)
    {
        Result<Token> result = await _tokenService.ExchangeToken(code, redirectUri);
        return result.IsSuccess ? Ok(result.Value) : Unauthorized(result.Errors);
    }
    
    [HttpPost]
    [Route("refresh")]
    public async Task<IActionResult> RefreshToken([FromHeader] string refreshToken, [FromHeader] string accessToken)
    {
        Result<Token> result = await _tokenService.RefreshToken(refreshToken, accessToken);
        return result.IsSuccess ? Ok(result.Value) : Unauthorized(result.Errors);
    }
}