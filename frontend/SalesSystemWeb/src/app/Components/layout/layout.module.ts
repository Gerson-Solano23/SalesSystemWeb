import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { LayoutRoutingModule } from './layout-routing.module';
import { DashboardComponent } from './Pages/dashboard/dashboard.component';
import { UserComponent } from './Pages/user/user.component';
import { ProductComponent } from './Pages/product/product.component';
import { SaleComponent } from './Pages/sale/sale.component';
import { SaleHistoryComponent } from './Pages/sale-history/sale-history.component';
import { ReportComponent } from './Pages/report/report.component';
import { SharedModule } from '../../Reusable/shared/shared.module';
import { ModalUserComponent } from './Modals/modal-user/modal-user.component';
import { ModalProductComponent } from './Modals/modal-product/modal-product.component';
import { ModalSaleDetailComponentm } from './Modals/modal-sale-detail/modal-sale-detail.component';
import { MessageComponent } from './Modals/message/message.component';
import { FilesComponent } from './Pages/files/files.component';
import { DataPredictionComponent } from './Pages/data-prediction/data-prediction.component';



@NgModule({
  declarations: [
    DashboardComponent,
    UserComponent,
    ProductComponent,
    SaleComponent,
    SaleHistoryComponent,
    ReportComponent,
    ModalUserComponent,
    ModalProductComponent,
    ModalSaleDetailComponentm,
    MessageComponent,
    FilesComponent,
    DataPredictionComponent
  ],
  imports: [
    CommonModule,
    LayoutRoutingModule,
    SharedModule
  ]
})
export class LayoutModule { }
