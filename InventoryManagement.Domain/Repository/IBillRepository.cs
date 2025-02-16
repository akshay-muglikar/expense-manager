using System;
using InventoryManagement.Domain.Model;

namespace InventoryManagement.Domain.Repository;

public interface IBillRepository
{
    Task<Bill> GetByIdAsync(int id);
    Task<IEnumerable<Bill>> GetAllAsync();
    Task AddAsync(Bill bill);
    Task UpdateAsync(Bill bill);
    Task DeleteAsync(int id);
    Task AddBillItems(List<BillItem> billItems);
    Task RemoveBillItems(int billId);
    Task<List<BillItem>> getBillItems(int id);
    Task<List<Bill>> GetAllAsync(DateTime? start, DateTime? end);
}
