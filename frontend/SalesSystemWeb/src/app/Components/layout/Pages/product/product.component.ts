import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatDialog } from '@angular/material/dialog';
import { ModalProductComponent } from '../../Modals/modal-product/modal-product.component';

import { Product } from '../../../../Interfaces/product';
import { ProductService } from '../../../../Services/product.service';
import { UtilityService } from '../../../../Reusable/utility.service';

import Swal from 'sweetalert2';
import { MessageComponent } from '../../Modals/message/message.component';
import { MessageModal } from '../../../../Interfaces/messaModal';
@Component({
  selector: 'app-product',
  templateUrl: './product.component.html',
  styleUrls: ['./product.component.css', '../../../../../styles.scss'],
})
export class ProductComponent implements OnInit, AfterViewInit {
  columsTable: string[] = ['Consecutivo', 'Name', 'Category', 'Stock', 'Price', 'Status', 'Actions'];
  dataProducts: Product[] = [];  
  dataListProducts = new MatTableDataSource<Product>(this.dataProducts);
  pageIndex: number = 0;
  pageSize: number = 10;
  messageData: MessageModal = {} as MessageModal;
  @ViewChild(MatPaginator) paginator!: MatPaginator;  

  constructor(
    private dialog: MatDialog,
    private productService: ProductService,
    private utilityService: UtilityService,
  ) {}
  getStockCero(listProducts: Product[]): Product[] {
      console.log('LIST PRODUCTS: ', listProducts);
      const listFilter = listProducts.filter(product => product.stock === 0);
      console.log('LIST listFilter: ', listFilter);
      return listFilter;
  }
  openModal(list:  Product[]): void {
    this.messageData.title = 'Products';
    const listFilter = this.getStockCero(list);
    if (listFilter.length > 0) {
      this.messageData.data = listFilter;
      console.log('MESSAGE DATA: ', this.messageData.data);
      this.dialog.open(MessageComponent, {
        width: '600px',
        height: '600px',
        data : this.messageData
      });    
    }
   return;
  }
  getListProducts() {
    this.productService.listProducts().subscribe({
      next: (response) => {
        if (response.status) {
          this.dataProducts = response.data;
          console.log('LIST PRODUCTS getListProducts: ', this.dataProducts);
          this.dataListProducts.data = this.dataProducts; // Actualiza los datos de la tabla
          this.dataListProducts.paginator = this.paginator; // Asigna el paginador después de actualizar los datos
          this.openModal(this.dataProducts);
        } else {
          this.utilityService.ShowAlert("Products not found", 'Error');
        }
      },
      error: (error) => {
        console.error("Error loading products: ", error);
      }
    });  
  }

  ngOnInit(): void {
    this.utilityService.onlyRolOneAndTwo();
    this.getListProducts();
    
    
    
  }

  ngAfterViewInit(): void {
    this.dataListProducts.paginator = this.paginator;
    
  }

  filterResearch(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataListProducts.filter = filterValue.trim().toLowerCase();
  }

  createProduct() {
    this.dialog.open(ModalProductComponent, {
      disableClose: true
    }).afterClosed().subscribe(result => {
      if (result === "true") {
        this.getListProducts();     
      }
    });
  }

  updateProduct(product: Product) {
    this.dialog.open(ModalProductComponent, {
      disableClose: true,
      data: product
    }).afterClosed().subscribe(result => {
      if (result === "true") {
        this.getListProducts();     
      }
    });
  }

  deleteProduct(product: Product) {
    Swal.fire({
      title: 'Are you sure to remove this product?',
      text: product.name,
      icon: 'warning',
      confirmButtonColor: '#3085d6',
      confirmButtonText: 'Yes, remove it!',
      showCancelButton: true,
      cancelButtonColor: '#d33',
      cancelButtonText: 'Cancel'
    }).then((result) => {
      if (result.isConfirmed) {
        this.productService.deleteproduct(product.idProduct).subscribe({
          next: (resultData) => {
            if (resultData.status) {
              this.utilityService.ShowAlert('Product was successfully removed', 'success');
              this.getListProducts();
              this.paginator.firstPage();
            } else {
              this.utilityService.ShowAlert('Product could not be removed', 'error');
            }
          },
          error: (e) => {
            console.error("Error deleting product: ", e);
          }
        });
      }
    });
  }
  onPageChange(event: PageEvent) {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
  }

  getConsecutive(index: number): number {
    return index + this.pageIndex * this.pageSize + 1;
  }
}
