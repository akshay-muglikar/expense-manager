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

    [HttpGet("client")]
    public async Task<IActionResult> GetClient()
    {
        var claims = User?.Claims.SingleOrDefault(c => c.Type=="client_id").Value;
        Console.WriteLine($"-----------------{claims}");
        var at = await _loginService.GetClient(claims);
        Console.WriteLine($"-------------------{at?.Name??"NoName"}");
        return Ok(at);
    }
}
