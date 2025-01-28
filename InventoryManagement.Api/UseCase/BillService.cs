using System;
using InventoryManagement.Domain.Model;
using InventoryManagement.Domain.Repository;

namespace InventoryManagement.Api.UseCase;

public class BillService
{
    private readonly IBillRepository _billRepository;

    public BillService(IBillRepository billRepository)
    {
        _billRepository = billRepository;
    }

    public async Task<Bill> GetByIdAsync(int id)
    {
        return await _billRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Bill>> GetAllAsync()
    {
        return await _billRepository.GetAllAsync();
    }

    public async Task AddAsync(Bill bill)
    {
        // Calculate the bill amount based on BillItems
        bill.CalculatedBillAmount = bill.BillItems.Sum(bi => bi.Amount) - bill.Discount;
        await _billRepository.AddAsync(bill);
    }

    public async Task UpdateAsync(Bill bill)
    {
        // Recalculate the bill amount during an update
        bill.CalculatedBillAmount = bill.BillItems.Sum(bi => bi.Amount) - bill.Discount;
        await _billRepository.UpdateAsync(bill);
    }

    public async Task DeleteAsync(int id)
    {
        await _billRepository.DeleteAsync(id);
    }
}

