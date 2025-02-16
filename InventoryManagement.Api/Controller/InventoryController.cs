using System;
using AutoMapper;
using InventoryManagement.Api.Contracts;
using InventoryManagement.Api.UseCase;
using InventoryManagement.Domain.Model;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Api.Controller;

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
}
