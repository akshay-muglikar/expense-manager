using System;
using System.Text.Json;
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
public class ItemController : ControllerBase
{
    private readonly ItemService _itemService;
    private readonly IMapper _mapper;

    public ItemController(ItemService itemService, IMapper mapper)
    {
        _itemService = itemService;
        _mapper = mapper;
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _itemService.GetByIdAsync(id);
        if (item == null)
            return NotFound();

        return Ok(item);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _itemService.GetAllAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> Add(ItemModel itemModel)
    {

        await _itemService.AddAsync(_mapper.Map<Item>(itemModel));
        return Created();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ItemModel itemModel)
    {
        await _itemService.UpdateAsync(_mapper.Map<Item>(itemModel));
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _itemService.DeleteAsync(id);
        return NoContent();
    }
    [HttpGet("download")]
    public async Task<IActionResult> Download()
    {
        var csv = await _itemService.GenerateCsvAsync(); ;
        return File(csv, "text/csv", "items.csv");
    }
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        using var stream = file.OpenReadStream();
        (int added, int updated) = await _itemService.UploadCsv(stream);
        return Ok(new
        {
            Added = added,
            Updated = updated
        });
    }

    [HttpPost("scan")]
    public async Task<IActionResult> Scan([FromBody] ScanRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Barcode))
            return BadRequest("Invalid scan request.");

        var clientId = Request.HttpContext?.User?.FindFirst("client_id")?.Value;

        await _itemService.ScanItemAsync(request.Barcode, clientId);
        return Ok();
    }
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var history = await _itemService.GetItemHistoryAsync();
        return Ok(history);
    }

}

public class ScanRequest
{
    public string Barcode { get; set; }
}