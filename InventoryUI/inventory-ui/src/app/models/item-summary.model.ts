

export interface ItemSummary{
totalItems : number
totalInventoryValue  : number
averageItemValue : number
lowStockCount  : number
outOfStockCount  : number
lowStockItems  : ItemList[]
}
export interface ItemList{
    itemName : string
    currentQuantity : number
    price : number
}

              