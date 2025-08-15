using AutoMapper;
using InventoryManagement.Api.Contracts;
using InventoryManagement.Api.Provider;
using InventoryManagement.Domain.Model;
using InventoryManagement.Domain.Repository;

namespace InventoryManagement.Api.UseCase;

public class SupplierService
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IMapper _mapper;
    private readonly string _user;

    public SupplierService(ISupplierRepository supplierRepository, IMapper mapper,
    UserServiceProvider userService)
    {
        _supplierRepository = supplierRepository;
        _mapper = mapper;
        _user = userService.GetUsername();
    }

    public async Task<SupplierModel?> GetByIdAsync(int id)
    {
        var supplier = await _supplierRepository.GetByIdAsync(id);
        return _mapper.Map<SupplierModel?>(supplier);
    }

    public async Task<IEnumerable<SupplierModel>> GetAllAsync()
    {
        var suppliers = await _supplierRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<SupplierModel>>(suppliers);
    }

    public async Task AddAsync(SupplierModel supplier)
    {
        var entity = _mapper.Map<Supplier>(supplier);
        await _supplierRepository.AddAsync(entity, _user);
    }

    internal async Task<IEnumerable<ExpenseModel>> GetExpensesBySupplierIdAsync(int id)
    {
        var expenses = await _supplierRepository.GetExpensesBySupplierIdAsync(id);
        return _mapper.Map<IEnumerable<ExpenseModel>>(expenses);
    }

    internal async Task UpdateAsync(SupplierModel supplierModel)
    {
        var supplier = _mapper.Map<Supplier>(supplierModel);
        await _supplierRepository.UpdateAsync(supplier, _user);
    }
}