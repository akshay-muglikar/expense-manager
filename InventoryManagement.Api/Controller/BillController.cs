using System;
using AutoMapper;
using InventoryManagement.Api.Contracts;
using InventoryManagement.Api.UseCase;
using InventoryManagement.Domain.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Api.Controller;

[Authorize]
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
    public async Task<IActionResult> AddBill(BillModel bill)
    {
        bill = await _billService.AddAsync(bill);
        return Ok(bill);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBill(int id, BillModel bill)
    {
        await _billService.UpdateAsync(bill);
        return NoContent();
    }
    [HttpPut("{id}/add")]
    public async Task<IActionResult> AddBillitem(int id, BillItemModel bill)
    {
        await _billService.Add(id, bill);
        return Ok();
    }
    [HttpPut("{id}/remove")]
    public async Task<IActionResult> RemoveBillItem(int id, [FromQuery] int billItemId)
    {
        await _billService.DeleteAsync(id, billItemId);
        return Ok();
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

    [HttpGet("{id}/message")]
    public async Task<IActionResult> Send(int id)
    {
        await _billService.SendMessage(id);
        return Ok();

    }
    // [HttpGet("download/{id}/url")]
    // public async Task<IActionResult> DownloadBillUrl(int id)
    // {
    //     return Ok(await _billService.GenerateDownloadLink(id));
    // }

    [AllowAnonymous]
    [HttpGet("download/{clientId}/{id}")]
    public async Task<IActionResult> DownloadBill(int id, Guid clientId, [FromQuery] string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            return BadRequest();
        }
        return Ok(await _billService.GeneratePdf(id, clientId, apiKey));

    }

    [HttpPost("add-with-items")]
    public async Task<IActionResult> AddBillWithItems([FromBody] AddBillRequest request)
    {
        if (request == null || request.BillItems == null || request.BillItems.Count == 0)
            return BadRequest("Bill and BillItems are required");
        var result = await _billService.AddBillWithItemsAsync(request);
        return Ok(result);
    }
    [HttpPost("update-with-items/{id}")]
    public async Task<IActionResult> UpdateBillWithItems([FromRoute] int id, [FromBody] AddBillRequest request)
    {
        if (request == null || request.BillItems == null || request.BillItems.Count == 0)
            return BadRequest("Bill and BillItems are required");
        var result = await _billService.UpdateBillWithItemsAsync(id,request);
        return Ok(result);
    }
}

