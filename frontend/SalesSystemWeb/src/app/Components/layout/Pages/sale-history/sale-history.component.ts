import { Component, OnInit, AfterViewInit, ViewChild, viewChild } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatDialog } from '@angular/material/dialog';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { MAT_DATE_FORMATS } from '@angular/material/core';
import moment from 'moment';
import { ModalSaleDetailComponentm } from '../../Modals/modal-sale-detail/modal-sale-detail.component';

import { Sale } from '../../../../Interfaces/sale';
import { SaleService } from '../../../../Services/sale.service';
import { UtilityService } from '../../../../Reusable/utility.service';



export const MY_FORMATS = {
  parse:{
    dateInput: 'DD/MM/YYYY'
  },
  display:{
    dateInput:'DD/MM/YYYY',
    monthYearLabel: 'MMMM YYYY'
  }
}
@Component({
  selector: 'app-sale-history',
  templateUrl: './sale-history.component.html',
  styleUrls: ['./sale-history.component.css', '../../../../../styles.scss'],
  providers:[
    {provide: MAT_DATE_FORMATS, useValue: MY_FORMATS}
  ]
})
export class SaleHistoryComponent implements OnInit, AfterViewInit{

  
  searchForm: FormGroup;
  searchOptions:any[] = [
    {value: 'date', description: 'By Dates'},
    {value: 'number', description: 'By Sale Number'},
  ];
  tablesColumns: string[] = ['Count', 'RegistryDate', 'DocumentNumber', 'PaymentType', 'Total', 'Actions'];
  initialData: Sale[] = [];
  dataListSales = new MatTableDataSource(this.initialData);

  pageIndex: number = 0;
  pageSize: number = 10;

  @ViewChild(MatPaginator) paginator!: MatPaginator;  
  
  constructor(
    private form: FormBuilder,
    private dialog: MatDialog,
    private saleService: SaleService,
    private utilityService: UtilityService
  ) {
    this.searchForm = this.form.group({
      searchBy:['date'],
      number: [''],
      dateStart: [''],
      dateEnd: ['']
    });

    this.searchForm.get('searchBy')?.valueChanges.subscribe(value=>{
      this.searchForm.patchValue({
        nuumber: '',
        dateStart: '',
        dateEnd: ''
      });
    });

  }
  ngOnInit(): void {
    
  }
  ngAfterViewInit(): void {
    this.dataListSales.paginator = this.paginator;
  }

  filterResearh(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataListSales.filter = filterValue.trim().toLowerCase();
  }

  searchSales(){
    var startDate = moment(this.searchForm.value.dateStart,'DD/MM/YYYY');
    var endDate = moment(this.searchForm.value.dateEnd,'DD/MM/YYYY');
    if(this.searchForm.value.searchBy === 'date'){
     

      if (!startDate.isValid() || !endDate.isValid()) {
        this.utilityService.ShowAlert('Please insert a correct date', 'Error');
        return;    
      }

      if(startDate>endDate){
        this.utilityService.ShowAlert('The start date must be less than the end date', 'Error');
        return;
      }
    }
    var startDateString = moment(this.searchForm.value.dateStart).format('DD/MM/YYYY');
    var endDateString = moment(this.searchForm.value.dateEnd).format('DD/MM/YYYY');
    this.saleService.history(
      this.searchForm.value.searchBy, 
      this.searchForm.value.number, 
      startDateString, endDateString).subscribe({
        next: (response) => {
          if(response.status){
            this.initialData = response.data;
            this.dataListSales.data = this.initialData;
            this.dataListSales.paginator = this.paginator;
          }else{
            this.utilityService.ShowAlert('Data not found', 'Error');
          }
        },
        error: (error) => {
         console.log(error);
        }
      });
  }

  showSaleDetail(sale: Sale){
    this.dialog.open(ModalSaleDetailComponentm, {
      data: sale,
      disableClose: true,
      width: '700px'
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
