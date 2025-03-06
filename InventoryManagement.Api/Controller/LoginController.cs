using System;
using System.IdentityModel.Tokens.Jwt;
using InventoryManagement.Api.Contracts;
using InventoryManagement.Api.UseCase;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Api.Controller;

[ApiController]
[Route("api/[controller]")]
public class LoginController: ControllerBase
{
    private readonly LoginService _loginService;

    public LoginController(LoginService loginService){
        _loginService = loginService;
    }

    [HttpPost]
    public async Task<IActionResult> Login(UserModel user)
    {
        var at = await _loginService.Login(user);
        if(at!=null){
            return Ok(new { AccessToken = new JwtSecurityTokenHandler().WriteToken(at)});
        }
        return Unauthorized("Invalid credentials");
    }
}
