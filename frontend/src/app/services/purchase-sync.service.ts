import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { OfflinePurchaseRecord } from '../models/purchase-bill.model';
import { OfflinePurchaseStorageService } from './offline-purchase-storage.service';
import { PurchaseApiService } from './purchase-api.service';

@Injectable({ providedIn: 'root' })
export class PurchaseSyncService {
  private readonly api = inject(PurchaseApiService);
  private readonly storage = inject(OfflinePurchaseStorageService);

  private syncInFlight: Promise<void> | null = null;

  constructor() {
    if (typeof window !== 'undefined') {
      window.addEventListener('online', () => {
        void this.syncPending();
      });
    }
  }

  /** Call after saves and on startup when online. */
  syncPending(): Promise<void> {
    if (this.syncInFlight) {
      return this.syncInFlight;
    }
    if (typeof navigator !== 'undefined' && !navigator.onLine) {
      return Promise.resolve();
    }

    this.syncInFlight = (async () => {
      const pending = await this.storage.getPending();
      for (const record of pending) {
        await this.syncOne(record);
      }
    })()
      .catch((err) => console.error('Purchase sync failed', err))
      .finally(() => {
        this.syncInFlight = null;
      });

    return this.syncInFlight;
  }

  private async syncOne(record: OfflinePurchaseRecord): Promise<void> {
    try {
      if (record.mode === 'update' && record.serverId != null) {
        const updated = await firstValueFrom(
          this.api.updatePurchaseBill(record.serverId, { lines: record.lines })
        );
        await this.storage.markSynced(record.localKey, updated.id);
        return;
      }

      const created = await firstValueFrom(
        this.api.createPurchaseBill({ syncKey: record.syncKey, lines: record.lines })
      );
      await this.storage.markSynced(record.localKey, created.id);
    } catch (e) {
      console.error('Sync record failed', record.localKey, e);
    }
  }
}
