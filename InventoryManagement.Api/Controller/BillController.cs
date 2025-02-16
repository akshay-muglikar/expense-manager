using System;
using AutoMapper;
using InventoryManagement.Api.Contracts;
using InventoryManagement.Api.UseCase;
using InventoryManagement.Domain.Model;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Api.Controller;

[ApiController]
[Route("api/[controller]")]
public class BillController : ControllerBase
{
    private readonly BillService _billService;
    private readonly IMapper _mapper;

    public BillController(BillService billService, IMapper mapper)
    {
        _billService = billService;
        _mapper = mapper;
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
    public async Task<IActionResult> GetAll([FromQuery] DateTime? start, [FromQuery] DateTime? end)
    {
        var bills = await _billService.GetAllAsync(start, end);
        return Ok(bills);
    }

    [HttpPost]
    public async Task<IActionResult> Add(BillModel bill)
    {
        bill = await _billService.AddAsync(bill);
        return Ok(bill);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, BillModel bill)
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

    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(int id)
    {
        return Ok(await _billService.GeneratePdf(id));
        
    }
}

