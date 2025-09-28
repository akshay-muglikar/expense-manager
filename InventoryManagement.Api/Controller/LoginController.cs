using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using InventoryManagement.Api.Contracts;
using InventoryManagement.Api.Provider;
using InventoryManagement.Api.UseCase;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Api.Controller;

[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    private readonly LoginService _loginService;
    private readonly UserServiceProvider _userServiceProvider;

    public LoginController(LoginService loginService, UserServiceProvider userServiceProvider)
    {
        _loginService = loginService;
        _userServiceProvider = userServiceProvider;
    }

    [HttpPost]
    public async Task<IActionResult> Login(UserModel user)
    {
        var at = await _loginService.Login(user);
        if (at != null)
        {
            return Ok(new { AccessToken = new JwtSecurityTokenHandler().WriteToken(at) });
        }
        return Unauthorized("Invalid credentials");
    }
    [HttpGet("user")]
    public async Task<IActionResult> GetUser()
    {
        var username = _userServiceProvider.GetUsername();
        var clientId = _userServiceProvider.GetClientId();
        var at = await _loginService.GetClient(clientId);

        return Ok(new { Username = username, Client = at });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(NewUserModel user)
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
    [HttpPost("register/client")]
    public async Task<IActionResult> RegisterClient(Register client)
    {
        await _loginService.RegisterClient(client);
        return Ok();
    }

    [HttpGet("client")]
    public async Task<IActionResult> GetClient()
    {
        var claims = User?.Claims.SingleOrDefault(c => c.Type == "client_id").Value;
        Console.WriteLine($"-----------------{claims}");
        var at = await _loginService.GetClient(claims);
        Console.WriteLine($"-------------------{at?.Name ?? "NoName"}");
        return Ok(at);
    }


    [HttpGet("download/{clientId}")]
    public async Task<IActionResult> DownloadDatabase([FromRoute] Guid clientId)
    {
        var stream = await _loginService.DownloadClientDatabase(clientId);
        if (stream == null)
        {
            return NotFound("No database found");
        }
        return File(stream, "application/octet-stream", "database.db");
    }

    [HttpGet("client-details")]
    public async Task<IActionResult> GetClientById()
    {
        var claims = User?.Claims.SingleOrDefault(c => c.Type == "client_id").Value;
        Console.WriteLine($"-----------------{claims}");
        var at = await _loginService.GetClientDetails(Guid.Parse(claims));

        Console.WriteLine($"-------------------{at?.Name ?? "NoName"}");
        return Ok(at);
    }

    [HttpPost("client-details")]
    public async Task<IActionResult> UpdateClientDetails([FromBody] ClientDetailsModel model)
    {
        var claims = User?.Claims.SingleOrDefault(c => c.Type == "client_id").Value;
        Console.WriteLine($"-----------------{claims}");
        if (model?.ClientId != Guid.Parse(claims))
        {
            return BadRequest("Client ID mismatch");
        }
        await _loginService.UpdateClientDetails(model);
        return Ok();
    }
    [HttpPost("upload-logo")]
    public async Task<IActionResult> UploadLogo(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");
        var claims = User?.Claims.SingleOrDefault(c => c.Type == "client_id").Value;
        using var stream = file.OpenReadStream();
        await _loginService.UploadLogo(Guid.Parse(claims), stream);
        return Ok();
    }
}
