import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { MAT_DATE_FORMATS } from '@angular/material/core';
import moment from 'moment';
import * as XLSX from 'xlsx';
import { Report } from '../../../../Interfaces/report';

import { SaleService } from '../../../../Services/sale.service';
import { UtilityService } from '../../../../Reusable/utility.service';
import { Sale } from '../../../../Interfaces/sale';
import { FileDownloadService } from '../../../../Services/file-download.service';
import { LoadingService } from '../../../../Services/loading.service';
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
  selector: 'app-report',
  templateUrl: './report.component.html',
  styleUrls: ['./report.component.css', '../../../../../styles.scss'],
  template: '<p>Fecha actual: {{ formattedDate }}</p>',
  providers:[
    {provide: MAT_DATE_FORMATS, useValue: MY_FORMATS}
  ]
})
export class ReportComponent {
  searchform: FormGroup;
  searchOptions: any[] = [
    {value: 'date', description: 'By Dates'},
    {value: 'number', description: 'By Sale Number'},
  ];
  pageIndex: number = 0;
  pageSize: number = 10;
  idUserInSession: number = -1;
  tableColums: string[] = ['Count', 'RegistryDate', 'DocumentNumber', 'PaymentType', 'TotalSale', 'Product','Price','Quantity','TotalProduct'];
  listReportsSales: Report[] = [];
  dataSaleReport = new MatTableDataSource(this.listReportsSales);
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  

  constructor(
    private form: FormBuilder,
    private saleService: SaleService,
    private utilityService: UtilityService,
    private FileDownloadService: FileDownloadService,
    public loadingService: LoadingService
  ) {
    this.searchform = this.form.group({
      dateStart: ['', Validators.required], 
      dateEnd: ['', Validators.required]
    });
  }
  ngOnInit(): void {
    this.utilityService.onlyRolOneAndTwo();
  }

  ngAfterViewInit(): void {
    this.dataSaleReport.paginator = this.paginator;   

    this.idUserInSession = this.utilityService.getUserSession().idUser;
  }

  searchSales(){
    const startDate = moment(this.searchform.value.dateStart, 'DD/MM/YYYY');
    const endDate = moment(this.searchform.value.dateEnd, 'DD/MM/YYYY');
    
    if (!startDate.isValid() || !endDate.isValid()) {
      this.utilityService.ShowAlert('Please insert a correct date', 'Error');
      return;    
    }
    
    if (startDate.isAfter(endDate)) {
      this.utilityService.ShowAlert('The start date must be less than the end date', 'Error');
      return;
    }
    var startDateString = moment(this.searchform.value.dateStart).format('DD/MM/YYYY');
    var endDateString = moment(this.searchform.value.dateEnd).format('DD/MM/YYYY');
    this.saleService.Report(startDateString, endDateString, this.idUserInSession).subscribe({
      next:(response)=>{
        if (response.status) {
          this.loadingService.active();
          this.listReportsSales = response.data;
          this.dataSaleReport = new MatTableDataSource(this.listReportsSales);
          this.dataSaleReport.paginator = this.paginator;
        }else{
          this.listReportsSales = [];
          this.dataSaleReport = new MatTableDataSource(this.listReportsSales);
          this.utilityService.ShowAlert('Data not Found', 'Opps');
        }
      },
      error:(error)=>{}
    });
  }

  exportExcel(){
    const wb = XLSX.utils.book_new();
    
    const ws = XLSX.utils.json_to_sheet(this.listReportsSales);
    XLSX.utils.book_append_sheet(wb, ws, 'Report_GreateShop');
    XLSX.writeFile(wb, 'Report_GreateShop.xlsx');
  }

  downloadExcelFile() {
    this.loadingService.show(); 
    
    this.FileDownloadService.downloadExcel(this.idUserInSession).subscribe({
      next: (response) => {
        if (response.status) {
          const byteCharacters = atob(response.data);
          const byteNumbers = new Array(byteCharacters.length);
          for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
          }
          const byteArray = new Uint8Array(byteNumbers);
          const blob = new Blob([byteArray], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
      
          // Crear un enlace de descarga y hacer clic en él
          const link = document.createElement('a');
          link.href = window.URL.createObjectURL(blob);
          const currentDate = new Date();
          const formattedDate = moment(currentDate).format('YYYY-MM-DD');
          link.download = 'Report-'+formattedDate+'.xlsx';
          link.click();
          window.URL.revokeObjectURL(link.href);  // Limpieza del objeto URL
          this.loadingService.hide();
        }
      },
      error: (error) => {
        this.utilityService.ShowAlert('Error downloading the file', 'Error');
        this.loadingService.hide();
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