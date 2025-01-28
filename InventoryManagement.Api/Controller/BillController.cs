using System;
using InventoryManagement.Api.UseCase;
using InventoryManagement.Domain.Model;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Api.Controller;

[ApiController]
[Route("api/[controller]")]
public class BillController : ControllerBase
{
    private readonly BillService _billService;

    public BillController(BillService billService)
    {
        _billService = billService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var bill = await _billService.GetByIdAsync(id);
        if (bill == null)
            return NotFound();

        return Ok(bill);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var bills = await _billService.GetAllAsync();
        return Ok(bills);
    }

    [HttpPost]
    public async Task<IActionResult> Add(Bill bill)
    {
        await _billService.AddAsync(bill);
        return CreatedAtAction(nameof(GetById), new { id = bill.Id }, bill);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Bill bill)
    {
        await _billService.UpdateAsync(bill);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _billService.DeleteAsync(id);
        return NoContent();
    }
}

