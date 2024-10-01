import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MessageModal } from '../../../../Interfaces/messaModal';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { ViewChild } from '@angular/core';
import { Product } from '../../../../Interfaces/product';
@Component({
  selector: 'app-message',
  templateUrl: './message.component.html',
  styleUrl: './message.component.css'
})
export class MessageComponent {
  productsList: Product[] = [];
  tableColums: string[] = ['Count','Product', 'Stock'];
  products = new MatTableDataSource(this.productsList);
  pageIndex: number = 0;
  pageSize: number = 10;
  @ViewChild(MatPaginator) paginator!: MatPaginator; 

  constructor(
    public dialogRef: MatDialogRef<MessageComponent>,
    @Inject(MAT_DIALOG_DATA) public data: MessageModal
  ) {
    this.products.data = data.data;
    console.log('DATA: ', this.data);
    this.products.paginator = this.paginator;
  }

  closeModal(): void {
    this.dialogRef.close();
  }

  ngOnInit(): void {
    this.products.paginator = this.paginator;
  }
  ngAfterViewInit(): void {
    this.products.paginator = this.paginator;
  
  }

  onPageChange(event: PageEvent) {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
  }

  getConsecutive(index: number): number {
    return index + this.pageIndex * this.pageSize + 1;
  }
}
