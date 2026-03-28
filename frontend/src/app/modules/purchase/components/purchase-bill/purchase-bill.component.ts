import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormArray,
  FormBuilder,
  FormGroup,
  Validators
} from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subscription, catchError, firstValueFrom, forkJoin, of } from 'rxjs';
import { ItemDto } from '../../../../models/item.model';
import { LocationDto } from '../../../../models/location.model';
import {
  OfflinePurchaseRecord,
  PurchaseBillLineInput,
  PurchaseBillListItem,
  PurchaseBillResponse
} from '../../../../models/purchase-bill.model';
import { OfflinePurchaseStorageService } from '../../../../services/offline-purchase-storage.service';
import { PurchaseApiService } from '../../../../services/purchase-api.service';
import { PurchaseSyncService } from '../../../../services/purchase-sync.service';

@Component({
  selector: 'app-purchase-bill',
  standalone: false,
  templateUrl: './purchase-bill.component.html',
  styleUrl: './purchase-bill.component.scss'
})
export class PurchaseBillComponent implements OnInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(PurchaseApiService);
  private readonly snack = inject(MatSnackBar);
  private readonly offline = inject(OfflinePurchaseStorageService);
  private readonly sync = inject(PurchaseSyncService);

  readonly isOnline = signal(typeof navigator !== 'undefined' ? navigator.onLine : true);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  items: ItemDto[] = [];
  locations: LocationDto[] = [];
  billHeaders: PurchaseBillListItem[] = [];

  currentBillId: number | null = null;
  currentCreatedAt: string | null = null;
  lastSyncKey: string | null = null;

  readonly summary = signal({
    totalItems: 0,
    totalQty: 0,
    grossCostPreDiscount: 0,
    totalDiscountAmount: 0,
    totalCostAfterDiscount: 0,
    totalSelling: 0
  });

  readonly pendingQueue = signal<OfflinePurchaseRecord[]>([]);

  /** Shared mat-autocomplete: which control is driving the filter */
  acMode: 'entry' | 'row' = 'entry';
  acRowIndex = 0;

  form: FormGroup = this.fb.group({
    entry: this.lineGroup(),
    lines: this.fb.array([])
  });

  private sub = new Subscription();

  get entry(): FormGroup {
    return this.form.get('entry') as FormGroup;
  }

  get lines(): FormArray {
    return this.form.get('lines') as FormArray;
  }

  ngOnInit(): void {
    window.addEventListener('online', this.onOnline);
    window.addEventListener('offline', this.onOffline);
    this.sub.add(
      this.form.valueChanges.subscribe(() => {
        this.recomputeSummary();
      })
    );
    void this.bootstrap();
  }

  ngOnDestroy(): void {
    window.removeEventListener('online', this.onOnline);
    window.removeEventListener('offline', this.onOffline);
    this.sub.unsubscribe();
  }

  private readonly onOnline = () => {
    this.isOnline.set(true);
    void this.refreshPending();
    void this.sync.syncPending();
  };

  private readonly onOffline = () => this.isOnline.set(false);

  billSubtitle(): string {
    if (this.currentBillId != null) {
      return `Bill #${this.currentBillId} · ${this.currentCreatedAt ? new Date(this.currentCreatedAt).toLocaleString() : ''}`;
    }
    return 'New document';
  }

  async bootstrap(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);
    try {
      const pack = await firstValueFrom(
        forkJoin({
          items: this.api.getItems(),
          locations: this.api.getLocations(),
          bills: this.api.listPurchaseBills().pipe(catchError(() => of([] as PurchaseBillListItem[])))
        })
      );
      this.items = pack.items;
      this.locations = pack.locations;
      this.billHeaders = pack.bills;
      this.resetEntryRow();
      this.recomputeSummary();
      await this.refreshPending();
      await this.sync.syncPending();
    } catch {
      this.error.set('Could not load master data. Check API and database connection.');
      this.snack.open('Failed to load data from server', 'Dismiss', { duration: 4000 });
    } finally {
      this.loading.set(false);
    }
  }

  async refreshPending(): Promise<void> {
    const all = await this.offline.listAll();
    this.pendingQueue.set(all.filter((r) => r.syncStatus === 'Pending'));
  }

  lineGroup(): FormGroup {
    return this.fb.group({
      id: [null as number | null],
      itemId: [null as number | null, Validators.required],
      itemSearch: ['', Validators.required],
      locationId: ['', Validators.required],
      cost: [0, [Validators.required, Validators.min(0)]],
      price: [0, [Validators.required, Validators.min(0)]],
      quantity: [1, [Validators.required, Validators.min(1)]],
      discountPercent: [0, [Validators.min(0), Validators.max(100)]]
    });
  }

  resetEntryRow(): void {
    this.entry.reset({
      id: null,
      itemId: null,
      itemSearch: '',
      locationId: this.locations[0]?.locationId ?? '',
      cost: 0,
      price: 0,
      quantity: 1,
      discountPercent: 0
    });
  }

  addLineFromEntry(): void {
    this.entry.markAllAsTouched();
    if (this.entry.invalid) {
      this.snack.open('Complete the line entry (item, batch, costs, qty)', 'Dismiss', { duration: 3500 });
      return;
    }
    const v = this.entry.getRawValue();
    const g = this.lineGroup();
    g.patchValue({
      id: null,
      itemId: v.itemId,
      itemSearch: v.itemSearch,
      locationId: v.locationId,
      cost: Number(v.cost),
      price: Number(v.price),
      quantity: Number(v.quantity),
      discountPercent: Number(v.discountPercent ?? 0)
    });
    this.lines.push(g);
    this.resetEntryRow();
    this.snack.open('Line added', 'OK', { duration: 1200 });
  }

  removeLine(index: number): void {
    this.lines.removeAt(index);
  }

  setAcEntry(): void {
    this.acMode = 'entry';
  }

  setAcRow(i: number): void {
    this.acMode = 'row';
    this.acRowIndex = i;
  }

  filterItemsAc(): ItemDto[] {
    return this.acMode === 'entry' ? this.filterItemsEntry() : this.filterItemsRow(this.acRowIndex);
  }

  onAcSelect(item: ItemDto): void {
    if (this.acMode === 'entry') {
      this.selectItemEntry(item);
    } else {
      this.selectItemRow(this.acRowIndex, item);
    }
  }

  filterItemsEntry(): ItemDto[] {
    const raw = this.entry.get('itemSearch')?.value ?? '';
    const v = String(raw).toLowerCase().trim();
    if (!v) {
      return this.items.slice(0, 40);
    }
    return this.items.filter((i) => i.itemName.toLowerCase().includes(v)).slice(0, 40);
  }

  filterItemsRow(idx: number): ItemDto[] {
    const raw = this.lines.at(idx)?.get('itemSearch')?.value ?? '';
    const v = String(raw).toLowerCase().trim();
    if (!v) {
      return this.items.slice(0, 40);
    }
    return this.items.filter((i) => i.itemName.toLowerCase().includes(v)).slice(0, 40);
  }

  selectItemEntry(item: ItemDto): void {
    this.entry.patchValue({
      itemId: item.itemId,
      itemSearch: item.itemName
    });
  }

  selectItemRow(idx: number, item: ItemDto): void {
    const g = this.lines.at(idx);
    g.patchValue({
      itemId: item.itemId,
      itemSearch: item.itemName
    });
  }

  displayLocation(locId: string): string {
    return this.locations.find((l) => l.locationId === locId)?.locationName ?? locId;
  }

  lineTotalCost(ctrl: AbstractControl): number {
    const v = ctrl.value;
    const cost = Number(v.cost ?? 0);
    const qty = Number(v.quantity ?? 0);
    const disc = Number(v.discountPercent ?? 0);
    return cost * qty * (1 - disc / 100);
  }

  lineDiscountAmount(ctrl: AbstractControl): number {
    const v = ctrl.value;
    const cost = Number(v.cost ?? 0);
    const qty = Number(v.quantity ?? 0);
    const disc = Number(v.discountPercent ?? 0);
    return cost * qty * (disc / 100);
  }

  lineGrossCost(ctrl: AbstractControl): number {
    const v = ctrl.value;
    return Number(v.cost ?? 0) * Number(v.quantity ?? 0);
  }

  lineTotalSelling(ctrl: AbstractControl): number {
    const v = ctrl.value;
    return Number(v.price ?? 0) * Number(v.quantity ?? 0);
  }

  lineMargin(ctrl: AbstractControl): number {
    const v = ctrl.value;
    return Number(v.price ?? 0) - Number(v.cost ?? 0);
  }

  entryTotalCost(): number {
    return this.lineTotalCost(this.entry);
  }

  entryTotalSelling(): number {
    return this.lineTotalSelling(this.entry);
  }

  entryMargin(): number {
    return this.lineMargin(this.entry);
  }

  private recomputeSummary(): void {
    let totalQty = 0;
    let totalSelling = 0;
    let totalCostNet = 0;
    let gross = 0;
    let discSum = 0;
    for (let i = 0; i < this.lines.length; i++) {
      const c = this.lines.at(i);
      totalQty += Number(c.get('quantity')?.value ?? 0);
      totalSelling += this.lineTotalSelling(c);
      totalCostNet += this.lineTotalCost(c);
      gross += this.lineGrossCost(c);
      discSum += this.lineDiscountAmount(c);
    }
    this.summary.set({
      totalItems: this.lines.length,
      totalQty,
      grossCostPreDiscount: gross,
      totalDiscountAmount: discSum,
      totalCostAfterDiscount: totalCostNet,
      totalSelling
    });
  }

  newBill(): void {
    this.currentBillId = null;
    this.currentCreatedAt = null;
    this.lastSyncKey = null;
    this.lines.clear();
    this.resetEntryRow();
    this.form.markAsPristine();
    this.error.set(null);
    this.snack.open('New bill started', 'OK', { duration: 2000 });
  }

  async loadBill(id: number): Promise<void> {
    this.loading.set(true);
    this.error.set(null);
    try {
      const bill = await firstValueFrom(this.api.getPurchaseBill(id));
      this.applyLoadedBill(bill);
      this.snack.open(`Loaded bill #${id}`, 'OK', { duration: 2000 });
    } catch (e) {
      console.error(e);
      this.showErrorSnack(this.messageFromHttpError(e, 'Failed to load bill'));
    } finally {
      this.loading.set(false);
    }
  }

  private applyLoadedBill(bill: PurchaseBillResponse): void {
    this.currentBillId = bill.id;
    this.currentCreatedAt = bill.createdAt;
    this.lastSyncKey = null;
    this.lines.clear();
    for (const line of bill.lines) {
      const g = this.lineGroup();
      g.patchValue({
        id: line.id,
        itemId: line.itemId,
        itemSearch: line.itemName,
        locationId: line.locationId,
        cost: line.cost,
        price: line.price,
        quantity: line.quantity,
        discountPercent: line.discountPercent
      });
      this.lines.push(g);
    }
    this.resetEntryRow();
    this.recomputeSummary();
  }

  private buildPayload(): { valid: boolean; lines: PurchaseBillLineInput[] } {
    if (this.lines.length === 0) {
      this.snack.open('Add at least one line item', 'Dismiss', { duration: 3500 });
      return { valid: false, lines: [] };
    }

    let anyInvalid = false;
    for (const c of this.lines.controls) {
      const g = c as FormGroup;
      g.markAllAsTouched();
      if (g.invalid) {
        anyInvalid = true;
      }
    }
    if (anyInvalid) {
      return { valid: false, lines: [] };
    }

    const lines: PurchaseBillLineInput[] = this.lines.controls.map((c) => {
      const v = (c as FormGroup).getRawValue();
      return {
        id: v.id ?? null,
        itemId: v.itemId,
        locationId: v.locationId,
        cost: Number(v.cost),
        price: Number(v.price),
        quantity: Number(v.quantity),
        discountPercent: Number(v.discountPercent ?? 0)
      };
    });
    return { valid: true, lines };
  }

  async save(): Promise<void> {
    const built = this.buildPayload();
    if (!built.valid) {
      if (this.lines.length > 0) {
        this.snack.open('Fix validation errors in the grid', 'Dismiss', { duration: 3500 });
      }
      return;
    }

    if (!navigator.onLine) {
      await this.saveOfflineInternal(built.lines);
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    try {
      if (this.currentBillId != null) {
        const updated = await firstValueFrom(
          this.api.updatePurchaseBill(this.currentBillId, { lines: built.lines })
        );
        this.applyLoadedBill(updated);
        this.snack.open('Purchase bill updated', 'OK', { duration: 2500 });
      } else {
        const syncKey = crypto.randomUUID();
        const created = await firstValueFrom(
          this.api.createPurchaseBill({ syncKey, lines: built.lines })
        );
        this.lastSyncKey = syncKey;
        this.applyLoadedBill(created);
        this.snack.open('Purchase bill saved', 'OK', { duration: 2500 });
      }
      await this.reloadBillList();
    } catch (e) {
      console.error(e);
      const msg = this.messageFromHttpError(
        e,
        'Save failed — try Save offline or verify API is running on port 5139.'
      );
      this.error.set(msg);
      this.showErrorSnack(msg);
    } finally {
      this.loading.set(false);
    }
  }

  async saveOffline(): Promise<void> {
    const built = this.buildPayload();
    if (!built.valid) {
      return;
    }
    await this.saveOfflineInternal(built.lines);
  }

  private async saveOfflineInternal(lines: PurchaseBillLineInput[]): Promise<void> {
    const localKey = crypto.randomUUID();
    const syncKey = this.lastSyncKey ?? crypto.randomUUID();
    const record: OfflinePurchaseRecord = {
      localKey,
      syncKey,
      serverId: this.currentBillId,
      syncStatus: 'Pending',
      mode: this.currentBillId != null ? 'update' : 'create',
      lines,
      label: `Bill ${this.currentBillId ?? 'new'} · ${new Date().toLocaleString()}`,
      updatedAt: new Date().toISOString()
    };
    await this.offline.upsert(record);
    await this.refreshPending();
    this.snack.open('Stored offline — will sync when online', 'OK', { duration: 3500 });
  }

  async syncNow(): Promise<void> {
    this.loading.set(true);
    try {
      await this.sync.syncPending();
      await this.refreshPending();
      await this.reloadBillList();
      this.snack.open('Sync completed', 'OK', { duration: 2500 });
    } catch (e) {
      console.error(e);
      this.showErrorSnack(this.messageFromHttpError(e, 'Sync failed — check console and API'));
    } finally {
      this.loading.set(false);
    }
  }

  private async reloadBillList(): Promise<void> {
    try {
      this.billHeaders = await firstValueFrom(this.api.listPurchaseBills());
    } catch {
      /* ignore */
    }
  }

  exportPdf(): void {
    if (this.currentBillId == null) {
      this.snack.open('Save the bill first to export PDF', 'Dismiss', { duration: 3500 });
      return;
    }
    window.open(this.api.pdfUrl(this.currentBillId), '_blank');
  }

  netTotalDisplay(): number {
    const s = this.summary();
    return s.totalSelling;
  }

  /** RFC 7807 Problem Details, ASP.NET validation, or plain text bodies. */
  private messageFromHttpError(err: unknown, fallback: string): string {
    if (err instanceof HttpErrorResponse) {
      const body = err.error;
      if (typeof body === 'string' && body.trim()) {
        return this.truncateMessage(body.trim());
      }
      if (body && typeof body === 'object') {
        const o = body as Record<string, unknown>;
        const detail = o['detail'];
        if (typeof detail === 'string' && detail.trim()) {
          return this.truncateMessage(detail.trim());
        }
        const title = o['title'];
        if (typeof title === 'string' && title.trim()) {
          return this.truncateMessage(title.trim());
        }
        const message = o['message'];
        if (typeof message === 'string' && message.trim()) {
          return this.truncateMessage(message.trim());
        }
        const nested = o['error'];
        if (typeof nested === 'string' && nested.trim()) {
          return this.truncateMessage(nested.trim());
        }
        if (nested && typeof nested === 'object') {
          const em = (nested as { message?: string }).message;
          if (typeof em === 'string' && em.trim()) {
            return this.truncateMessage(em.trim());
          }
        }
      }
      if (err.status === 0) {
        return 'Cannot reach server. Check that the API is running and CORS allows this origin.';
      }
      return err.message || `Request failed (HTTP ${err.status})`;
    }
    if (err instanceof Error && err.message) {
      return err.message;
    }
    return fallback;
  }

  private truncateMessage(s: string, max = 320): string {
    return s.length <= max ? s : `${s.slice(0, max)}…`;
  }

  private showErrorSnack(message: string): void {
    this.snack.open(message, 'Dismiss', {
      duration: 12000,
      panelClass: ['error-snackbar'],
      horizontalPosition: 'center',
      verticalPosition: 'bottom'
    });
  }
}
