using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InventoryManagement.Api.Contracts;
using InventoryManagement.Api.Provider;
using InventoryManagement.Domain.Model;
using InventoryManagement.Domain.Repository;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace InventoryManagement.Api.UseCase;

public class LoginService
{
    private readonly ILoginReporsitory _loginReporsitory;
    protected readonly UserServiceProvider userServiceProvider;

    public LoginService(ILoginReporsitory loginReporsitory, UserServiceProvider userServiceProvider)
    {
        this.userServiceProvider = userServiceProvider;
        _loginReporsitory = loginReporsitory;
    }

    public async Task<JwtSecurityToken> Login(UserModel user){
        var userModel = await _loginReporsitory.GetUser(user.Username, user.Password);
        if(userModel != null){
            Console.WriteLine("test");
            return GenerateAccessToken(user.Username, userModel.ClientId);
        }
        return null;
    }

    public async Task<ClientModel> GetClient(string? value)
    {
        
        return await _loginReporsitory.GetClient(Guid.Parse(value));
    }
    
    public async Task<List<ClientModel>> GetAllClients()
    {
         if(userServiceProvider.GetUsername()== "admin")
        {
            throw new UnauthorizedAccessException("Admin user cannot be registered through this endpoint");
        }
        return await _loginReporsitory.GetAllClients();
    }   
    public async Task<List<User>> GetUsersAsync(Guid id)
    {
         if(userServiceProvider.GetUsername()== "admin")
        {
            throw new UnauthorizedAccessException("Admin user cannot be registered through this endpoint");
        }
        return await _loginReporsitory.GetUsersAsync(id);
    }

    private JwtSecurityToken GenerateAccessToken(string userName, Guid clientId)
    {
        // Create user claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userName),
            new Claim("client_id", clientId.ToString()),
            // Add additional claims as needed (e.g., roles, etc.)
        };

        // Create a JWT
        var token = new JwtSecurityToken(
            issuer: "http://localhost:5050",
            audience: "http://localhost:4200",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(240), // Token expiration time
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("JwtSettings:SecretKeyNew123JwtSettings:SecretKeyNew123")),
                SecurityAlgorithms.HmacSha256)
        );

        return token;
    }

    public async Task Register(UserModel user)
    {
        if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
        {
            throw new ArgumentException("Username and password must be provided");
        }
        if(userServiceProvider.GetUsername()== "admin")
        {
            throw new UnauthorizedAccessException("Admin user cannot be registered through this endpoint");
        }
        var userModel = new User
        {
            Username = user.Username,
            Password = user.Password,
            ClientId = Guid.NewGuid()
        };

        await _loginReporsitory.CreateUser(userModel);
    }

    internal async Task ContactUs(Contact contact)
    {
        await _loginReporsitory.ContactUs(contact);
    }
}
