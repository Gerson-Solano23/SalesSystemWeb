import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LayoutComponent } from './layout.component';
import { DashboardComponent } from './Pages/dashboard/dashboard.component';
import { UserComponent } from './Pages/user/user.component';
import { ProductComponent } from './Pages/product/product.component';
import { SaleComponent } from './Pages/sale/sale.component';
import { ReportComponent } from './Pages/report/report.component';
import { HistorySalesComponent } from './Pages/history-sales/history-sales.component';

const routes: Routes = [{
  path: '',
  component: LayoutComponent, 
  children:[
    {path: 'dashboard', component: DashboardComponent},
    {path: 'user', component: UserComponent},
    {path: 'products', component: ProductComponent},
    {path: 'sale', component: SaleComponent},
    {path: 'history-Sale', component: HistorySalesComponent},
    {path: 'reports', component: ReportComponent},
  ]
}];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class LayoutRoutingModule { }
