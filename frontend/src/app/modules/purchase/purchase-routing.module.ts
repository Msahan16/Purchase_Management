import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PurchaseBillComponent } from './components/purchase-bill/purchase-bill.component';

const routes: Routes = [{ path: '', component: PurchaseBillComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PurchaseRoutingModule {}
