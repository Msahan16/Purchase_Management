import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'purchase' },
  {
    path: 'purchase',
    loadChildren: () => import('./modules/purchase/purchase.module').then((m) => m.PurchaseModule)
  }
];
