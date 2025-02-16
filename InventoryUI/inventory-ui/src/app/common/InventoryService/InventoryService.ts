import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Item } from "../../contracts/item.model";

@Injectable({providedIn: 'root'})
export class InventoryService {
  constructor(private http: HttpClient) {
  }

  getInventory() {
    return this.http.get<Item[]>('/api/item');
  }

  addToInventory(item: Item) {
    return this.http.post('/api/item', item);
  }

  searchInventory(query: string) {
    return this.http.get<Item[]>('/api/inventory/search', { params: { query } });
  }

  updateInventory(item: Item) {
    return this.http.put('/api/item/'+item.id, item);
  }
}