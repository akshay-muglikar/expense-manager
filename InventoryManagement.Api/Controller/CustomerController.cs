using InventoryManagement.Api.UseCase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Api.Controller;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly CustomerService _customerService;
    public CustomerController(CustomerService customerService)
    {
        _customerService = customerService;
    }
    [HttpGet]
    public async Task<IActionResult> GetAllCustomers()
    {
        var customers = await _customerService.GetAllCustomersAsync();
        return Ok(customers);
    }

    [HttpGet("bills")]
    public async Task<IActionResult> GetCustomerById([FromQuery] string? name, [FromQuery] string? mobile)
    {
        
        var customerBills = await _customerService.GetCustomerBills(name, mobile??"");
        return Ok(customerBills);
    }
}