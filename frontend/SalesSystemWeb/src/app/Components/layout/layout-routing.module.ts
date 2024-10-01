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

const routes: Routes = [{
  path: '',
  component: LayoutComponent,
  children:[
    {path:'dashboard', component: DashboardComponent},
    {path:'users', component: UserComponent},
    {path:'products', component: ProductComponent},
    {path:'sale', component: SaleComponent},
    {path:'history_sale', component: SaleHistoryComponent},
    {path:'reports', component: ReportComponent},
    {path:'files', component: FilesComponent},
    {path:'dataPrediction', component: DataPredictionComponent},
  ]
}];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class LayoutRoutingModule { }
