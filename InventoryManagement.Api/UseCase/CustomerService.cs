using AutoMapper;
using InventoryManagement.Api.Contracts;
using InventoryManagement.Domain.Repository;

namespace InventoryManagement.Api.UseCase;

public class CustomerService
{
    private readonly IBillRepository _billRepository;
    private readonly IMapper _mapper;

    public CustomerService(IBillRepository billRepository, IMapper mapper)
    {
        _mapper = mapper;
        _billRepository = billRepository;
    }

    public async Task<IEnumerable<CustomerModel>> GetAllCustomersAsync()
    {
        var customers = await _billRepository.GetAllCustomersAsync();
        return customers.Select(c => new CustomerModel
        {
            Name = c.Item1,
            Mobile = c.Item2
        });
    }
    public async Task<CustomerModel> GetCustomerBills(string name, string mobile)
    {
        var billItems = await _billRepository.GetCustomerBillsByNameAndMobileAsync(name, mobile);
         
        return new CustomerModel
        {
            Name = name,
            Mobile = mobile,
            Bills  = billItems.Select(x =>
            {
                var map = _mapper.Map<BillModel>(x.Item1);
                map.TotalAmount = x.Item2;
                return map;
            }).ToList()
        };
    }
}