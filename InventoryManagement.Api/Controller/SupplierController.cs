
using AutoMapper;
using InventoryManagement.Api.Contracts;
using InventoryManagement.Api.UseCase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Api.Controller;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SupplierController : ControllerBase
{
    private readonly SupplierService _supplierService;
    private readonly IMapper _mapper;

    public SupplierController(SupplierService supplierService, IMapper mapper)
    {
        _supplierService = supplierService;
        _mapper = mapper;
    }


    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var suppliers = await _supplierService.GetAllAsync();
        return Ok(suppliers);
    }
    [HttpPost]
    public async Task<IActionResult> AddSupplier(SupplierModel supplierModel)
    {
        await _supplierService.AddAsync(supplierModel);
        return Ok();
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSupplierExpenses(int id)
    {
        var expenses = await _supplierService.GetExpensesBySupplierIdAsync(id);
        return Ok(expenses);
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSupplier(int id, SupplierModel supplierModel)
    {
        await _supplierService.UpdateAsync(supplierModel);
        return NoContent();
    }
}