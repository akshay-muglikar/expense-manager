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
    [HttpPost("register")]
    public async Task<IActionResult> Register(UserModel user)
    {
        await _loginService.Register(user);
        return Ok();
    }
    [HttpGet("clients")]
    public async Task<IActionResult> GetAllClient()
    {
        var clients = await _loginService.GetAllClients();
        return Ok(clients);
    }
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(Guid id)
    {
        var users = await _loginService.GetUsersAsync(id);
        return Ok(users);
    }

    [HttpPost("contact")]
    public async Task<IActionResult> ContactUs(Contact contact)
    {
        await _loginService.ContactUs(contact);
        return Ok();
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
