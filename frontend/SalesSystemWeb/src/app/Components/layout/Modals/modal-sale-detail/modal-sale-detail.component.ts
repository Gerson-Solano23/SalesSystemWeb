import { Component, Inject , OnInit, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Sale } from '../../../../Interfaces/sale';
import { SaleDetail } from '../../../../Interfaces/sale-detail';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
@Component({
  selector: 'app-modal-sale-detail',
  templateUrl: './modal-sale-detail.component.html',
  styleUrl: './modal-sale-detail.component.css'
})
export class ModalSaleDetailComponentm implements OnInit {
  
  registryDate: string = '';
  documentNumber: string = '';
  paymentType: string = '';
  total:string = '';
  saleDetails: SaleDetail[] = [];
  tableColums: string[] = ['Count','Product', 'Quantity', 'Price', 'Total'];
  dataSaleDetails = new MatTableDataSource(this.saleDetails);
  pageIndex: number = 0;
  pageSize: number = 10;
  @ViewChild(MatPaginator) paginator!: MatPaginator;  
  constructor(@Inject(MAT_DIALOG_DATA) public sale: Sale) {
    this.registryDate = sale.dateRegistry!;
    this.documentNumber = sale.numberDocument!;
    this.paymentType = sale.paymentType;
    this.total = sale.total_Text;
    this.saleDetails = sale.saleDetails;
    this.dataSaleDetails.data = this.saleDetails;
    this.dataSaleDetails.paginator = this.paginator;
  }
  ngOnInit(): void {
    this.dataSaleDetails.paginator = this.paginator;
  }
  ngAfterViewInit(): void {
    this.dataSaleDetails.paginator = this.paginator;
  
  }

  onPageChange(event: PageEvent) {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
  }

  getConsecutive(index: number): number {
    return index + this.pageIndex * this.pageSize + 1;
  }
}

