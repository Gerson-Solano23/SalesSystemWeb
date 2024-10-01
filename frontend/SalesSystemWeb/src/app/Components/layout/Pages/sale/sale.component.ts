import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatTableDataSource } from '@angular/material/table';


import { ProductService } from '../../../../Services/product.service';
import { SaleService } from '../../../../Services/sale.service';
import { UtilityService } from '../../../../Reusable/utility.service';

import { Product } from '../../../../Interfaces/product';
import { Sale } from '../../../../Interfaces/sale';
import { SaleDetail } from '../../../../Interfaces/sale-detail';

import Swal from 'sweetalert2';
import { MatPaginator, PageEvent } from '@angular/material/paginator';

@Component({
  selector: 'app-sale',
  templateUrl: './sale.component.html',
  styleUrls: ['./sale.component.css', '../../../../../styles.scss']
})
export class SaleComponent{

  productList: Product[] = [];
  filterListProducts: Product[] = [];
  productListoTosale: SaleDetail[] = [];
  registryBlockButton: boolean = true;
  paymentType: string = 'Cash';
  totalPayment: number = 0;

  pageIndex: number = 0;
  pageSize: number = 10;

  selectedProduct!: Product;

  formSale: FormGroup;
  columsTable: string[] = ['Consecutivo', 'Product', 'Quantity', 'Price', 'Total', 'Actions'];
  dataSaleDetails = new MatTableDataSource(this.productListoTosale);

  getProductFiltered(search: any):Product[]{
    const searchValue = typeof search === 'string' ? search.toLocaleLowerCase() : search.name.toLocaleLowerCase();

    return this.productList.filter(item => item.name.toLocaleLowerCase().includes(searchValue));
  }
  @ViewChild(MatPaginator) paginator!: MatPaginator;  
  constructor(
    private form: FormBuilder,
    private productService: ProductService,
    private saleService: SaleService,
    private utilityService: UtilityService
  ) {
      this.formSale= this.form.group({
        product: ['', Validators.required],
        quantity: ['', Validators.required]
      });

      this.productService.listProducts().subscribe({
        next: (response) => {
          if (response.status) {
            const list = response.data as Product[];
            this.productList = list.filter(item => item.status === 1 && item.stock > 0); 
          }
        },
        error: (error) => {}
      }); 
      
      this.formSale.get('product')?.valueChanges.subscribe(value=>{
        this.filterListProducts = this.getProductFiltered(value); 
        console.log('Lista Filtrada', this.filterListProducts);
      });
  }
  // ngOnInit(): void {
  //   throw new Error('Method not implemented.');
  // }

  showProduct(product: Product):string{
    return product.name;
  }
  
  productForSale(event: any){
    this.selectedProduct = event.option.value;
  }

  addProductForSale(){
    const quantity = this.formSale.value.quantity;
    if(quantity <= 0){
      this.utilityService.ShowAlert('The quantity must be greater than 0', 'Error');
      return;
    }
    if (this.verifyStock(quantity)) {
      const price: number = parseFloat(this.selectedProduct.price);
      const total: number = quantity * price;
      this.totalPayment += total;
      const index = this.productListoTosale.findIndex(item => item.idProduct === this.selectedProduct.idProduct);
      if(index != -1){
        this.editProductOfList(index, quantity, total);
      }else{
        this.productListoTosale.push({
          idProduct: this.selectedProduct.idProduct,
          productDescription: this.selectedProduct.name,
          quantity: quantity,
          price: String(price.toFixed(2)),
          total: String(total.toFixed(2))
        });
      }
      
  
      this.dataSaleDetails.data = this.productListoTosale;
      this.dataSaleDetails.paginator = this.paginator;
      this.registryBlockButton = false;
  
      this.formSale.patchValue({
        product: '',
        quantity: ''
      });
  
      
    }
   
  }

  deleteProductForSale(productsale: SaleDetail){
    this.totalPayment -= parseFloat(productsale.total);
    this.productListoTosale = this.productListoTosale.filter(item => item.idProduct != productsale.idProduct);
    this.dataSaleDetails.data = this.productListoTosale;
    this.dataSaleDetails.paginator = this.paginator;
    this.paginator.firstPage();
    if (this.productListoTosale.length == 0) {
      this.totalPayment = 0;
    }

  }

  createSale(){
    if(this.filterListProducts.length > 0){
      this.registryBlockButton = true;
      const sale: Sale = {
        paymentType: this.paymentType,
        total_Text: String(this.totalPayment.toFixed(2)),
        saleDetails: this.productListoTosale,
        idSale: 0,
        numberDocument: '',
        dateRegistry: ''
      }
      this.saleService.create(sale).subscribe({
        next: (response) => {
          if(response.status){
            this.totalPayment = 0;
            this.productListoTosale = [];
            this.dataSaleDetails.data = this.productListoTosale;
            this.paginator.firstPage();
            Swal.fire({
              icon: 'success',
              title: 'Sale registry successfully',
              text: `Sale number: ${response.data.numberDocument}`,
            });
          }else{
            this.utilityService.ShowAlert('Sale could not be registered', 'Error');
          }
        },
        complete: () => {
          this.registryBlockButton = true;
        },
        error: (error) => {}
      });
    }
  }
  onPageChange(event: PageEvent) {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
  }

  getConsecutive(index: number): number {
    return index + this.pageIndex * this.pageSize + 1;
  }

  editProductOfList(index: number, quantity: number, total: number){
    if (index != -1) {
      var quantityOld = this.productListoTosale[index].quantity;
      if (this.verifyStock(quantityOld+quantity)) {
        this.productListoTosale[index].quantity += quantity;
        this.productListoTosale[index].total = String((parseInt(this.productListoTosale[index].total)+total).toFixed(2));
      }       
    }
  }

  verifyStock(quantity: number):boolean{
    if(this.selectedProduct.stock < quantity){
      this.utilityService.ShowAlert(`The quantity exceeds the stock "${this.selectedProduct.stock}"`, 'Error');
      return false;
    }
    return true;

  }
}
