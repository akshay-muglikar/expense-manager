export class Item {
    id: number;
    name: string;
    category: string;
    car: string;
    quantity: number;
    description: string;
    price: number;
  
    constructor(
      name: string,
      category: string,
      car: string,
      quantity: number,
      description: string,
      price: number,
      id: number
    ) {
      this.name = name;
      this.category = category;
      this.car = car;
      this.quantity = quantity;
      this.description = description;
      this.price = price;
      this.id = id;
    }
  }
  