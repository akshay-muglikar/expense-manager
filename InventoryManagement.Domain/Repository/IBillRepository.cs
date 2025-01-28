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
}
