using System;
using InventoryManagement.Domain.Model;

namespace InventoryManagement.Domain.Repository;

public interface IBillRepository
{
    Task<Bill> GetByIdAsync(int id);
    Task<IEnumerable<Bill>> GetAllAsync();
    Task AddAsync(Bill bill, string user);
    Task UpdateAsync(Bill bill, string user);
    Task DeleteAsync(int id, string user);
    Task AddBillItems(List<BillItem> billItems);
    Task RemoveBillItems(int billId);
    Task RemoveBillItem(BillItem billItem);
    Task<List<BillItem>> getBillItems(int id);
    Task<List<Bill>> GetAllAsync(DateTime? start, DateTime? end);
    Task AddBillWithItemsAsync(Bill bill, List<BillItem> billItems, string user);
}
