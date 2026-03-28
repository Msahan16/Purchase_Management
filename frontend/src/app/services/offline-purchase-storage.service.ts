import { Injectable } from '@angular/core';
import { DBSchema, IDBPDatabase, openDB } from 'idb';
import { OfflinePurchaseRecord } from '../models/purchase-bill.model';

interface PmDB extends DBSchema {
  bills: {
    key: string;
    value: OfflinePurchaseRecord;
    indexes: { 'by-status': SyncStatusIndex };
  };
}

type SyncStatusIndex = 'Pending' | 'Synced';

@Injectable({ providedIn: 'root' })
export class OfflinePurchaseStorageService {
  private readonly dbPromise: Promise<IDBPDatabase<PmDB>>;

  constructor() {
    this.dbPromise = openDB<PmDB>('purchase-management-db', 1, {
      upgrade(db) {
        const store = db.createObjectStore('bills', { keyPath: 'localKey' });
        store.createIndex('by-status', 'syncStatus');
      }
    });
  }

  async upsert(record: OfflinePurchaseRecord): Promise<void> {
    const db = await this.dbPromise;
    await db.put('bills', record);
  }

  async listAll(): Promise<OfflinePurchaseRecord[]> {
    const db = await this.dbPromise;
    return db.getAll('bills');
  }

  async getPending(): Promise<OfflinePurchaseRecord[]> {
    const db = await this.dbPromise;
    return db.getAllFromIndex('bills', 'by-status', 'Pending');
  }

  async markSynced(localKey: string, serverId: number): Promise<void> {
    const db = await this.dbPromise;
    const row = await db.get('bills', localKey);
    if (!row) {
      return;
    }
    row.syncStatus = 'Synced';
    row.serverId = serverId;
    row.updatedAt = new Date().toISOString();
    await db.put('bills', row);
  }

  async remove(localKey: string): Promise<void> {
    const db = await this.dbPromise;
    await db.delete('bills', localKey);
  }
}
