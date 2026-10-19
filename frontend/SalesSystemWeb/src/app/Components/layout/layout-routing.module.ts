import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DashboardComponent } from './Pages/dashboard/dashboard.component';
import { UserComponent } from './Pages/user/user.component';
import { ProductComponent } from './Pages/product/product.component';
import { SaleComponent } from './Pages/sale/sale.component';
import { SaleHistoryComponent } from './Pages/sale-history/sale-history.component';
import { ReportComponent } from './Pages/report/report.component';
import { LayoutComponent } from './layout.component';
import { FilesComponent } from './Pages/files/files.component';
import { DataPredictionComponent } from './Pages/data-prediction/data-prediction.component';
import { authGuard, roleGuard } from '../../Reusable/auth.guard';

// Roles mirror the API authorization policies of each page's endpoints.
const routes: Routes = [{
  path: '',
  component: LayoutComponent,
  canActivate: [authGuard],
  canActivateChild: [roleGuard],
  children:[
    {path:'dashboard', component: DashboardComponent, data: {roles: ['Admin']}},
    {path:'users', component: UserComponent, data: {roles: ['Admin']}},
    {path:'products', component: ProductComponent, data: {roles: ['Admin', 'Supervisor']}},
    {path:'sale', component: SaleComponent, data: {roles: ['Admin', 'Supervisor', 'Employee']}},
    {path:'history_sale', component: SaleHistoryComponent, data: {roles: ['Admin', 'Supervisor', 'Employee']}},
    {path:'reports', component: ReportComponent, data: {roles: ['Admin', 'Supervisor']}},
    {path:'files', component: FilesComponent, data: {roles: ['Admin', 'Supervisor']}},
    {path:'dataPrediction', component: DataPredictionComponent, data: {roles: ['Admin', 'Supervisor']}},
  ]
}];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class LayoutRoutingModule { }
