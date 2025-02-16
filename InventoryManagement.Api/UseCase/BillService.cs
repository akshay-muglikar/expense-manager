using System;
using AutoMapper;
using InventoryManagement.Api.Contracts;
using InventoryManagement.Api.Provider;
using InventoryManagement.Domain.Model;
using InventoryManagement.Domain.Repository;

namespace InventoryManagement.Api.UseCase;

public class BillService
{
    private readonly IBillRepository _billRepository;
    private readonly IItemRepository _itemRepository;

    private readonly IMapper _mapper;

    public BillService(IBillRepository billRepository, IMapper mapper, IItemRepository itemRepository)
    {
        _billRepository = billRepository;
        _mapper = mapper;
        _itemRepository = itemRepository;
    }


    public async Task<BillModel> GetByIdAsync(int id)
    {
        var bill =  await _billRepository.GetByIdAsync(id);
        var billItems  = await _billRepository.getBillItems(id);
       
        var billModel =  _mapper.Map<BillModel>(bill);
        var billItemMode = new List<BillItemModel>();
        foreach(var item in billItems)
        {
            billItemMode.Add(_mapper.Map<BillItemModel>(item));
        }
        billModel.BillItems = billItemMode;
        return billModel;
    }

    public async Task<IEnumerable<Bill>> GetAllAsync(DateTime? start, DateTime? end)
    {
        if(start ==null && end ==null){
            return await _billRepository.GetAllAsync();
        }else{
            return await _billRepository.GetAllAsync(start,end);
        }
    }

    public async Task<BillModel> AddAsync(BillModel billmodel)
    {
        Enum.TryParse(billmodel.Status, out BillStatus status);

        Bill bill = new Bill(){
            Name = billmodel.Name,
            Mobile = billmodel.Mobile,
            Discount = billmodel.Discount,
            Advance = billmodel.Advance,
            BillDate = billmodel.BillDate,
            status = status,
            User = "admin"
        };
        bill.CalculatedBillAmount = billmodel.BillItems.Sum(bi => int.Parse(bi.Amount)*bi.Quantity) - bill.Discount;

        await _billRepository.AddAsync(bill);
        List<BillItem> billItems = new();
        foreach(var billItem in billmodel.BillItems){
            var item = await _itemRepository.GetByIdAsync(billItem.ItemId);
            if(item == null && billItem.OtherItem != null){
               item = new Item(){
                   Name = billItem.OtherItem,
                   Price = int.Parse(billItem.Amount),
               };
               await _itemRepository.AddAsync(item);
            }
            if(item == null)
                throw new Exception("Item not found");  

            Console.WriteLine($"amount {billItem.Amount}");
            billItems.Add(new BillItem(){
                ItemId = item.Id,
                Quantity = billItem.Quantity,
                Amount = int.Parse(billItem.Amount),
                BillId = bill.Id
            });
        }
        await _billRepository.AddBillItems(billItems);

        // Calculate the bill amount based on BillItems
        return _mapper.Map<BillModel>(bill);
    }

    public async Task UpdateAsync(BillModel billModel)
    {
        var bill = await _billRepository.GetByIdAsync(billModel.Id);
        if (bill == null)
            throw new Exception("Bill not found");

        Enum.TryParse(billModel.Status, out BillStatus status);

        bill.Name = billModel.Name;
        bill.Mobile = billModel.Mobile;
        bill.Discount = billModel.Discount;
        bill.Advance = billModel.Advance;
        bill.BillDate = billModel.BillDate;
        bill.status = status;
        bill.User = "admin";


        bill.CalculatedBillAmount = billModel.BillItems.Sum(bi =>int.Parse(bi.Amount)*bi.Quantity) - bill.Discount;

        await _billRepository.UpdateAsync(bill);

          List<BillItem> billItems = new();
        foreach(var billItem in billModel.BillItems){
            var item = await _itemRepository.GetByIdAsync(billItem.ItemId);
            if(item == null && billItem.OtherItem != null){
               item = new Item(){
                   Name = billItem.OtherItem,
                   Price = int.Parse(billItem.Amount),
               };
               await _itemRepository.AddAsync(item);
            
            }
            if(item == null)
                throw new Exception("Item not found");
            billItems.Add(new BillItem(){
                ItemId = item.Id,
                Quantity = billItem.Quantity,
                Amount = int.Parse(billItem.Amount),
                BillId = bill.Id
            });
        }
        await _billRepository.RemoveBillItems(bill.Id);
        await _billRepository.AddBillItems(billItems);
    }
    

    public async Task DeleteAsync(int id)
    {
        await _billRepository.DeleteAsync(id);
    }

    public async Task<Stream> GeneratePdf(int id)
    {
        var bill = await _billRepository.GetByIdAsync(id);
        if (bill == null)
            throw new Exception("Bill not found");

        var billitems = await _billRepository.getBillItems(bill.Id);
        // Generate PDF
        return InvoiceProvider.GetInvoice(bill, billitems);

    }
}

