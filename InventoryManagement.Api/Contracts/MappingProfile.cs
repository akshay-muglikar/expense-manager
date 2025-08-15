using System;
using AutoMapper;
using InventoryManagement.Domain.Model;

namespace InventoryManagement.Api.Contracts;

 public class MappingProfile : Profile {
    public MappingProfile()
    {
        // Add as many of these lines as you need to map your objects
        CreateMap<Item, ItemModel>();
        CreateMap<ItemModel, Item>();

        CreateMap<Bill, BillModel>();
        CreateMap<BillModel, Bill>();

        CreateMap<BillItem, BillItemModel>();
        CreateMap<BillItemModel, BillItem>();

        CreateMap<GetBillModel, Bill>();
        CreateMap<Bill, GetBillModel>();

        CreateMap<GetAllBillModel, Bill>();
        CreateMap<Bill, GetAllBillModel>();

        CreateMap<SupplierModel, Supplier>();
        CreateMap<Supplier, SupplierModel>();
        
        CreateMap<ExpenseModel, Expense>()
            .ForMember(dest => dest.PaymentMode, opt => opt.MapFrom(src => Enum.Parse<PaymentMode>(src.PaymentMode)))
            .ForMember(dest => dest.ExpenseType, opt => opt.MapFrom(src => Enum.Parse<ExpenseType>(src.ExpenseType)));
        CreateMap<Expense, ExpenseModel>()
            .ForMember(dest => dest.PaymentMode, opt => opt.MapFrom(src => src.PaymentMode.ToString()))
            .ForMember(dest => dest.ExpenseType, opt => opt.MapFrom(src => src.ExpenseType.ToString()));

     }
 }