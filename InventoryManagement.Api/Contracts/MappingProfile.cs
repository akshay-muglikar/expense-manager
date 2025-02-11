using System;
using AutoMapper;
using InventoryManagement.Domain.Model;

namespace InventoryManagement.Api.Contracts;

 public class MappingProfile : Profile {
     public MappingProfile() {
         // Add as many of these lines as you need to map your objects
         CreateMap<Item, ItemModel>();
         CreateMap<ItemModel, Item>();
     }
 }