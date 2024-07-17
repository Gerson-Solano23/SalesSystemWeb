import { Component, OnInit, Inject } from '@angular/core';

import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

import { Category } from '../../../../Interfaces/category';
import { Product } from '../../../../Interfaces/product';
import { CategoryService } from '../../../../Services/category.service';
import { ProductService } from '../../../../Services/product.service';
import { UtilityService } from '../../../../Reusable/utility.service';

@Component({
  selector: 'app-modal-product',
  templateUrl: './modal-product.component.html',
  styleUrl: './modal-product.component.css'
})
export class ModalProductComponent implements OnInit{
  
  formProduct: FormGroup;
  actionTitle: string = 'Add';
  actionButton: string = 'Save';
  categoryList: Category[] = [];

  constructor(
    private nowModal: MatDialogRef<ModalProductComponent>,
    @Inject(MAT_DIALOG_DATA) public productData: Product,
    private form: FormBuilder,
    private categoryService: CategoryService,
    private productService: ProductService,
    private utilityService: UtilityService
  ) {
    this.formProduct = this.form.group({
      name: ['', Validators.required],
      idCategory: ['', Validators.required],
      stock: ['', Validators.required],
      price: ['', Validators.required],
      status: ['', Validators.required],
    });
    if (this.productData != null) {
      this.actionTitle = 'Edit';
      this.actionButton = 'Update';
    }

    this.categoryService.listCategories().subscribe({
      next: (response) => {
        if (response.status) {
          this.categoryList = response.data;
        }
      },
      error: (error) => {}
    });  
  }
  
  ngOnInit(): void {
    if (this.productData != null) {
      this.formProduct.patchValue({
        name: this.productData.name,
        idCategory: this.productData.idCategory,
        stock: this.productData.stock,
        price: this.productData.price.toString(),
        status: this.productData.status.toString()
      });
      
    }
  }

  createOrUpdateProduct() {
    const product: Product ={
      idProduct: this.productData == null ? 0 : this.productData.idProduct,
      name: this.formProduct.value.name,
      idCategory: this.formProduct.value.idCategory,
      categoryDescription:"",
      stock: this.formProduct.value.stock,
      price: this.formProduct.value.price.toString(),
      status: parseInt(this.formProduct.value.status)
    }
    if (this.productData == null) {
      this.productService.createProduct(product).subscribe({
        next:(response)=>{
          if(response.status){
            this.utilityService.ShowAlert('Product was successfully created', 'success');
            this.nowModal.close("true");
          }else{
            this.utilityService.ShowAlert("Product could not be created", 'error');
          }
        },
        error:(error)=>{
          this.utilityService.ShowAlert("Product could not be created, try again", 'error');
        }
      });
    }else{
      this.productService.updateProduct(product).subscribe({
        next:(response)=>{
          if(response.status){
            this.utilityService.ShowAlert('Product was successfully updated', 'success');
            this.nowModal.close("true");
          }else{
            this.utilityService.ShowAlert("Product could not be updated", 'error');
          }
        },
        error:(error)=>{
          this.utilityService.ShowAlert("Product could not be updated, try again", 'error');
        }
      });
    }
  }

}
