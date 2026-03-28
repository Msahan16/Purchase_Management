import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ItemDto } from '../models/item.model';
import { LocationDto } from '../models/location.model';
import {
  PurchaseBillListItem,
  PurchaseBillResponse,
  PurchaseBillSaveDto
} from '../models/purchase-bill.model';

@Injectable({ providedIn: 'root' })
export class PurchaseApiService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiUrl;

  getItems(): Observable<ItemDto[]> {
    return this.http.get<ItemDto[]>(`${this.base}/api/items`);
  }

  getLocations(): Observable<LocationDto[]> {
    return this.http.get<LocationDto[]>(`${this.base}/api/locations`);
  }

  listPurchaseBills(): Observable<PurchaseBillListItem[]> {
    return this.http.get<PurchaseBillListItem[]>(`${this.base}/api/purchase-bill`);
  }

  getPurchaseBill(id: number): Observable<PurchaseBillResponse> {
    return this.http.get<PurchaseBillResponse>(`${this.base}/api/purchase-bill/${id}`);
  }

  createPurchaseBill(body: PurchaseBillSaveDto): Observable<PurchaseBillResponse> {
    return this.http.post<PurchaseBillResponse>(`${this.base}/api/purchase-bill`, body);
  }

  updatePurchaseBill(id: number, body: PurchaseBillSaveDto): Observable<PurchaseBillResponse> {
    return this.http.put<PurchaseBillResponse>(`${this.base}/api/purchase-bill/${id}`, body);
  }

  pdfUrl(id: number): string {
    return `${this.base}/api/purchase-bill/${id}/pdf`;
  }
}
