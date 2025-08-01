using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InventoryManagement.Api.Contracts;
using InventoryManagement.Domain.Model;
using InventoryManagement.Domain.Repository;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace InventoryManagement.Api.UseCase;

public class LoginService
{
    private readonly ILoginReporsitory _loginReporsitory;

    public LoginService(ILoginReporsitory loginReporsitory){
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
}
