import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';

import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatDialog } from '@angular/material/dialog';
import { ModalProductComponent } from '../../Modals/modal-product/modal-product.component';

import { Product } from '../../../../Interfaces/product';
import { ProductService } from '../../../../Services/product.service';
import { UtilityService } from '../../../../Reusable/utility.service';

import Swal from 'sweetalert2';
@Component({
  selector: 'app-product',
  templateUrl: './product.component.html',
  styleUrl: './product.component.css'
})
export class ProductComponent implements OnInit, AfterViewInit{
  columsTable: string[] = ['Consecutivo', 'Name', 'Category', 'Stock', 'Price', 'Status','Actions'];
  dataProducts: Product[] = [];  
  dataListProducts = new MatTableDataSource(this.dataProducts);

  @ViewChild(MatPaginator) paginator!: MatPaginator;  

  constructor(
    private dialog: MatDialog,
    private productService: ProductService,
    private utilityService: UtilityService
    
  ) { }

  getListProducts() {
    this.productService.listProducts().subscribe({
      next: (response) => {
        if (response.status) {
          this.dataListProducts = response.data;
        }else{
          this.utilityService.ShowAlert("Products not found", 'Error');
        }
      },
      error: (error) => {}
    });  
  }

  ngOnInit(): void {
    this.getListProducts();
  }

  ngAfterViewInit(): void {
    this.dataListProducts.paginator = this.paginator;
  }

  filterResearh(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataListProducts.filter = filterValue.trim().toLowerCase();
  }

  createProduct(){
    this.dialog.open(ModalProductComponent, {
      disableClose: true
    }).afterClosed().subscribe(result=>{
      if (result === "true") {
        this.getListProducts();     
      }
    });
  }

  updateProduct(product: Product){
    this.dialog.open(ModalProductComponent, {
      disableClose: true,
      data: product
    }).afterClosed().subscribe(result=>{
      if (result === "true") {
        this.getListProducts();     
      }
    });
  }


  deleteProduct(product: Product){
    Swal.fire({
      title:'Are you sure to remove this user?',
      text: product.name,
      icon: 'warning',
      confirmButtonColor: '#3085d6',
      confirmButtonText: 'Yes, remove it!',
      showCancelButton: true,
      cancelButtonColor: '#d33',
      cancelButtonText: 'Cancel'
    }).then((result)=>{
      if (result.isConfirmed) {
        this.productService.deleteproduct(product.idProduct).subscribe({
          next:(resultData)=>{
            console.log('resultData', resultData);
            if (resultData.status) {
              this.utilityService.ShowAlert('Product was successfully removed', 'success');
              this.getListProducts();
            }else{
              this.utilityService.ShowAlert('Product could not be removed', 'error');
            }
          },
          error:(e)=>{}
        })
      }
    })
  }

}
