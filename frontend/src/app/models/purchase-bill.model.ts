export interface PurchaseBillLineInput {
  id?: number | null;
  itemId: number;
  locationId: string;
  cost: number;
  price: number;
  quantity: number;
  discountPercent: number;
}

export interface PurchaseBillSaveDto {
  syncKey?: string | null;
  lines: PurchaseBillLineInput[];
}

export interface PurchaseBillLineResponse {
  id: number;
  itemId: number;
  itemName: string;
  locationId: string;
  locationName: string;
  cost: number;
  price: number;
  quantity: number;
  discountPercent: number;
  lineTotalCost: number;
  lineTotalSelling: number;
}

export interface PurchaseBillResponse {
  id: number;
  createdAt: string;
  lines: PurchaseBillLineResponse[];
  totalItems: number;
  totalQuantity: number;
  totalAmount: number;
  totalCostAmount: number;
}

export interface PurchaseBillListItem {
  id: number;
  createdAt: string;
  lineCount: number;
  totalAmount: number;
}

export type SyncStatus = 'Pending' | 'Synced';

export interface OfflinePurchaseRecord {
  localKey: string;
  syncKey: string;
  serverId: number | null;
  syncStatus: SyncStatus;
  mode: 'create' | 'update';
  lines: PurchaseBillLineInput[];
  label: string;
  updatedAt: string;
}
