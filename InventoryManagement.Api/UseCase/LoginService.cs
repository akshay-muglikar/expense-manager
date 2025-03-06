using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InventoryManagement.Api.Contracts;
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
        if(await _loginReporsitory.IsValid(user.Username, user.Password)){
            Console.WriteLine("test");
            return GenerateAccessToken(user.Username);
        }
        return null;
    }

    private JwtSecurityToken GenerateAccessToken(string userName)
    {
        // Create user claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userName),
            // Add additional claims as needed (e.g., roles, etc.)
        };

        // Create a JWT
        var token = new JwtSecurityToken(
            issuer: "http://localhost:5050",
            audience: "http://localhost:4200",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(1), // Token expiration time
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("JwtSettings:SecretKeyNew123JwtSettings:SecretKeyNew123")),
                SecurityAlgorithms.HmacSha256)
        );

        return token;
    }
}
