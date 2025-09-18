using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InventoryManagement.Api.Config;
using InventoryManagement.Api.Contracts;
using InventoryManagement.Api.Provider;
using InventoryManagement.Domain.Model;
using InventoryManagement.Domain.Repository;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace InventoryManagement.Api.UseCase;

public class LoginService
{
    private readonly ILoginReporsitory _loginReporsitory;
    protected readonly UserServiceProvider userServiceProvider;
    private readonly string _connectionString;
    public LoginService(ILoginReporsitory loginReporsitory,
    UserServiceProvider userServiceProvider, IOptionsSnapshot<InventoryConfig> inventoryConfig)
    {
        this.userServiceProvider = userServiceProvider;
        _loginReporsitory = loginReporsitory;
        _connectionString = inventoryConfig.Value.ConnectionStrings.Database.Replace("{AppBase}", AppContext.BaseDirectory);
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

    public async Task Register(NewUserModel user)
    {
        if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
        {
            throw new ArgumentException("Username and password must be provided");
        }
        if(userServiceProvider.GetUsername()!= "admin")
        {
            throw new UnauthorizedAccessException("Only admin user can register new users");
        }
        if (user.Username == "admin")
        {
            throw new UnauthorizedAccessException("Admin user cannot be registered through this endpoint");
        }
        var userModel = new User
        {
            Username = user.Username,
            Password = user.Password,
            ClientId = user.ClientId
        };

        await _loginReporsitory.CreateUser(userModel);
    }

    internal async Task ContactUs(Contact contact)
    {
        await _loginReporsitory.ContactUs(contact);
    }

    internal async Task RegisterClient(Register client)
    {
        if (string.IsNullOrEmpty(client.Username) || string.IsNullOrEmpty(client.Password) || string.IsNullOrEmpty(client.Name))
        {
            throw new ArgumentException("Username, password, and name must be provided");
        }
        if(userServiceProvider.GetUsername()!= "admin")
        {
            throw new UnauthorizedAccessException("Only admin user can register new clients");
        }
        if (client.Username == "admin")
        {
            throw new UnauthorizedAccessException("Admin user cannot be registered through this endpoint");
        }
        
        var clientModel = new ClientModel
        {
            Name = client.Name,
            LicenseExpiryDate = DateTime.Now.AddMonths(3)
        };

        var userModel = new User
        {
            Username = client.Username,
            Password = client.Password,
            ClientId = clientModel.Id
        };
        var ClientDetails = new ClientDetails
        {
            Address = client.Address,
            GSTNumber = string.IsNullOrEmpty(client.GSTNumber)?"":client.GSTNumber,
            RegistrationDate = DateTime.Now,
        };

        await _loginReporsitory.RegisterClient(clientModel, userModel, ClientDetails);
    }

    internal async Task<Stream> DownloadClientDatabase(Guid clientId)
    {
        if (userServiceProvider.GetUsername() == "admin")
        {
            throw new UnauthorizedAccessException("Admin user cannot download client database through this endpoint");
        }

        var downloadPath = _connectionString.Replace("{clientId}", clientId.ToString())
                                .Replace("Data Source=", "");
        if (!File.Exists(downloadPath))
        {
            throw new FileNotFoundException("Database file not found", downloadPath);
        }

        return new FileStream(downloadPath, FileMode.Open, FileAccess.Read);
    }
}
