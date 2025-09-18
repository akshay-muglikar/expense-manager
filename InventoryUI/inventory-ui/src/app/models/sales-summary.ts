export interface SalesSummary{

    totalRevenue : number,
    totalOrders : number,
    averageOrderValue : number,
    topProducts : Itemsummary[]

}
export interface Itemsummary {
    itemName : string,
    totalQuantitySold: number,
    totalRevenue:number,
    numberOfOrders: number,
    averagePrice : number

}