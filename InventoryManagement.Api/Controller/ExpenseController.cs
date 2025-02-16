using System;
using InventoryManagement.Api.UseCase;
using InventoryManagement.Domain.Model;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Api.Controller;

[ApiController]
[Route("api/[controller]")]
public class ExpenseController : ControllerBase
{
    private readonly ExpenseService _expenseService;

    public ExpenseController(ExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var expense = await _expenseService.GetByIdAsync(id);
        if (expense == null)
            return NotFound();

        return Ok(expense);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DateTime? start, [FromQuery] DateTime? end)
    {
        var expenses = await _expenseService.GetAllAsync(start, end);
        return Ok(expenses);
    }

    [HttpPost]
    public async Task<IActionResult> Add(Expense expense)
    {
        await _expenseService.AddAsync(expense);
        return Created();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Expense expense)
    {
        if (id != expense.Id)
            return BadRequest("ID mismatch");

        await _expenseService.UpdateAsync(expense);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _expenseService.DeleteAsync(id);
        return NoContent();
    }
}

