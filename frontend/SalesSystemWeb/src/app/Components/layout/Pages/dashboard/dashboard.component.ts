import {ChangeDetectionStrategy, Component, ViewChild, ViewEncapsulation} from '@angular/core';
import {FormBuilder, FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import {provideMomentDateAdapter} from '@angular/material-moment-adapter';
import {MatDatepickerModule} from '@angular/material/datepicker';
import { Chart, registerables, Title } from 'chart.js';
import { Dashboard } from '../../../../Interfaces/dashboard';
import { DashboardService } from '../../../../Services/dashboard.service';
import { MatSelectChange } from '@angular/material/select';
import Swal from 'sweetalert2';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { MatDatepicker } from '@angular/material/datepicker';
import { DateAdapter } from '@angular/material/core';
import * as _moment from 'moment';
// tslint:disable-next-line:no-duplicate-imports
import {default as _rollupMoment, Moment} from 'moment';
import {MatInputModule} from '@angular/material/input';
import {MatFormFieldModule} from '@angular/material/form-field';
import {Sort, MatSortModule} from '@angular/material/sort';
import { UtilityService } from '../../../../Reusable/utility.service';
import { topProduct } from '../../../../Interfaces/topProducts';
import { Router } from '@angular/router';
Chart.register(...registerables);
const moment = _rollupMoment || _moment;

export const MY_FORMATS = {
  parse: {
    dateInput: 'MM/YYYY',
  },
  display: {
    dateInput: 'MM/YYYY',
    monthYearLabel: 'MMM YYYY',
    dateA11yLabel: 'LL',
    monthYearA11yLabel: 'MMMM YYYY',
  },
};
export const MY_FORMATSOFYEAR = {
  parse: {
    dateInput: 'YYYY',
  },
  display: {
    dateInput: 'YYYY',
    monthLabel: 'YYYY',
    monthYearLabel: 'YYYY',
    dateA11yLabel: 'YYYY',
    monthYearA11yLabel: 'YYYY',
  },
};
export const MY_CUSTOM_FORMATS = {
  parse: {
    dateInput: 'LL', // Interpreta la fecha en formato "Día Mes Año" (por ejemplo, "6 de agosto de 2024").
  },
  display: {
    dateInput: 'LL', // Muestra la fecha en el mismo formato "Día Mes Año".
    monthYearLabel: 'MM YYYY', // Muestra el mes abreviado y el año (por ejemplo, "Ago 2024").
    dateA11yLabel: 'LL', // Etiqueta de accesibilidad para la fecha completa.
    monthYearA11yLabel: 'MM YYYY', // Etiqueta de accesibilidad para el mes completo y el año.
  },
};

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css', '../../../../../styles.scss'],
  providers: [provideMomentDateAdapter(MY_FORMATS),
    provideMomentDateAdapter(MY_CUSTOM_FORMATS)
  ]
})
export class DashboardComponent {
  searchform: FormGroup;
  readonly date = new FormControl(moment());
  readonly dateYear = new FormControl(moment());
  @ViewChild('lastWeekTab') lastWeekTab: any;
  totalSales:string='0';
  totalRevenue:string='0';
  totalProducts:string='0';
  delayed:any;
  BarChart: any;
  LineChartRange: any;
  DoughnutChart: any;
  LineChart: any;
  listTableData: any[] = [];
  selectedMonth: number;
  selectedYear: number = 0;
  topProductsTitle = 'Top 10 Best Selling Products Last Week';
  topListProducts: topProduct[] = [];

  perYearFilter = 0;
  perMonthFilter: number[] = [];
  RangeDatesFilter: any[] = [];


  constructor(
    private form: FormBuilder,
    private dateAdapter: DateAdapter<Date>,
    private dashboardService: DashboardService,
    private utilityService: UtilityService
  ) {
    const currentDate = new Date();
    this.selectedMonth = currentDate.getMonth();
    this.dateAdapter.setLocale('en-GB');
    this.searchform = this.form.group({
      dateStart: ['', Validators.required], 
      dateEnd: ['', Validators.required]
    });
   }
   setMonthAndYear(normalizedMonthAndYear: Moment, datepicker: MatDatepicker<Moment>) {
    const ctrlValue = this.date.value ?? moment();
    ctrlValue.month(normalizedMonthAndYear.month());
    ctrlValue.year(normalizedMonthAndYear.year());
    this.perMonthFilter = [normalizedMonthAndYear.month()+1, normalizedMonthAndYear.year()];
    this.getSalesPerMonth(this.perMonthFilter[0], this.perMonthFilter[1]);
    this.date.setValue(ctrlValue);
    
    datepicker.close();
  }
  setYear(normalizedYear: Moment, datepicker: MatDatepicker<Moment>) {
    this.perYearFilter = normalizedYear.year();
    this.getSalesPerYear(this.perYearFilter);
    datepicker.close();
  }
  // setRangeDates(start: Moment, datepicker: MatDatepicker<Moment>, end: Moment, datepicker: MatDatepicker<Moment>){
  //   this.getSalesByRangedates(start, end);
  //   datepicker.close();
  // }
  handleLastWeekTabClick(event: MatTabChangeEvent): void {
    if (event.tab.textLabel === 'Last Week') {
      this.topProductsTitle = 'Top 10 Best Selling Products Last Week';
      console.log('Pestaña "Last Week" seleccionada');
      this.getsalesLastweek();
    }
    else if (event.tab.textLabel === 'Per Month') {
      this.topListProducts = [];
      this.getSalesPerMonth(this.perMonthFilter[0], this.perMonthFilter[1]);
      this.topProductsTitle = 'Top 10 Best Selling Products Per Month'; 
    }
    else if (event.tab.textLabel === 'Per Year') {
      this.topListProducts = [];
      this.topProductsTitle = 'Top 10 Best Selling Products Per Year';
      
      this.getSalesPerYear(this.perYearFilter);
    }
    else if (event.tab.textLabel === 'Range Dates') {
      this.topProductsTitle = 'Top 10 Best Selling Products in Range';
      this.topListProducts = [];
      this.getSalesByRangedates();
    }
   
  }
  showGraficBard(graficLabel:any[], graficData:any[]){
    if ( this.BarChart) {
      this.BarChart.destroy();
    }
    this.BarChart = new Chart('barChart', {
      type: 'bar',
      data:{
        labels: graficLabel,
        datasets:[{
          label:"# of Sales",
          data: graficData,
          backgroundColor: [
            'rgba(255, 99, 132, 0.2)',
            'rgba(255, 159, 64, 0.2)',
            'rgba(255, 205, 86, 0.2)',
            'rgba(75, 192, 192, 0.2)',
            'rgba(54, 162, 235, 0.2)',
            'rgba(153, 102, 255, 0.2)',
            'rgba(201, 203, 207, 0.2)'
          ],
          borderColor: [
            'rgb(255, 99, 132)',
            'rgb(255, 159, 64)',
            'rgb(255, 205, 86)',
            'rgb(75, 192, 192)',
            'rgb(54, 162, 235)',
            'rgb(153, 102, 255)',
            'rgb(201, 203, 207)'
          ],
          borderWidth: 1
        }]
      },
      options:{
        
        animation: {
          onComplete: () => {
            this.delayed = true;
          },
          delay: (context) => {
            let delay = 0;
            if (context.type === 'data' && context.mode === 'default' && !this.delayed) {
              delay = context.dataIndex * 300 + context.datasetIndex * 100;
            }
            return delay;
          },
        },
        maintainAspectRatio: false,
        responsive: true,
        scales:{
          x: {
            grid: {
                color: 'rgba(255, 255, 255, 0.1)' // Color de las líneas de la cuadrícula en el eje X
            }
        },
          y:{
            beginAtZero: true,
            ticks:{
              color: '#ccc6c6',
            },
            grid: {
              color: 'rgba(255, 255, 255, 0.1)' // Color de las líneas de la cuadrícula en el eje X
          }
          }
        },
        plugins:{
          title:{
            display: true,
            text: 'Sales Last Week',
            color: '#ccc6c6',
          }
        }
      }
    });
  }
 getsalesLastweek(){
    this.dashboardService.summary().subscribe({
      next: (response) => {
        if (response.status) {
          this.totalSales = response.data.totalSales;
          this.totalRevenue = response.data.totalRevenue;
          this.totalProducts = response.data.totalProducts;

          const listData: any[] = response.data.salesLastWeek;
          this.topListProducts = response.data.topProducts;
          console.log('Last Week: ', this.topListProducts);

          const labelTemp = listData.map((item) => item.date);
          const dataTemp = listData.map((item) => item.total);

          this.showGraficBard(labelTemp, dataTemp);
        }
      },
      error: (error) => {
        console.error(error);
      }
    });
  }
  ngOnInit(): void {
    this.getsalesLastweek();
    this.utilityService.onlyRolOne();
  }

  getSalesPerMonth(month: number, year: number) {
    this.dashboardService.MonthSummary(month, year).subscribe({
      next: (response) => {
        if (response.status) {
          this.totalSales = response.data.totalSales;
          this.totalRevenue = response.data.totalRevenue;
          this.totalProducts = response.data.totalProducts;
          const listData: any[] = response.data.monthlySales;
          this.topListProducts = response.data.topProducts;
          console.log('Monthly: ', this.topListProducts);

          const labelTemp = listData.map((item) => item.week);
          const dataTemp = listData.map((item) => item.total);

          this.showGraficDoughnut(labelTemp, dataTemp);

         
        }
      },
      error: (error) => {
        console.error(error);
      }
    });
  }

  showGraficDoughnut(graficLabel: any[], graficData: any[]) {
    if (this.DoughnutChart) {
        this.DoughnutChart.destroy();
    }
    this.DoughnutChart = new Chart('doughnutChart', {
        type: 'pie',
        data: {
            labels: graficLabel,
            datasets: [{
                label: "# Sales per Week",
                data: graficData,
                backgroundColor: [
                    'rgba(193, 71, 97, 1)',
                    'rgba(255, 159, 64, 1)',
                    'rgba(255, 205, 86, 1)',
                    'rgba(75, 192, 192, 1)',
                    'rgba(54, 162, 235, 1)',
                    'rgba(153, 102, 255, 1)',
                    'rgba(201, 203, 207, 1)'
                ],
                borderWidth: 1,
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                title: {
                    display: true,
                    text: 'Sales Distribution Per Week',
                    color: '#ccc6c6',
                },
            },
        }
    });
  }


  getSalesPerYear(year: number) {
    this.dashboardService.YearSummary(year).subscribe({
      next: (response) => {
        if (response.status) {
          this.totalSales = response.data.totalSales;
          this.totalRevenue = response.data.totalRevenue;
          this.totalProducts = response.data.totalProducts;
          const listData: any[] = response.data.yearlySales;
          this.topListProducts = response.data.topProducts;
          console.log('Yearly: ', this.topListProducts);
          const labelTemp = listData.map((item) => item.month);
          const dataTemp = listData.map((item) => item.total);

          this.showGraficLine(labelTemp, dataTemp);

         
        }
      },
      error: (error) => {
        console.error(error);
      }
    });
  }

  showGraficLine(graficLabel: any[], graficData: any[]) {
    if (this.LineChart) {
      this.LineChart.destroy();
    }
    this.LineChart = new Chart('lineChart', {
      type: 'line',
      data: {
        labels: graficLabel,
        datasets: [{
          label: "# Sales per Month",
          data: graficData,
          fill: false,
          borderColor: 'rgba(130, 181, 214, 1)',
          pointStyle:'circle',
          pointRadius:10,
          pointHoverRadius:15,
          tension: 0
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        interaction: {
          intersect: false,
          axis: 'x'
        },
        scales: {
          x: {
            grid: {
              color: 'rgba(255, 255, 255, 0.1)' // Color de las líneas de la cuadrícula en el eje X
            }
          },
          y: {
            grid: {
              color: 'rgba(255, 255, 255, 0.1)' // Color de las líneas de la cuadrícula en el eje Y
            },
            min: 0 // Establecer el valor mínimo del eje Y en 0
          }
        }
      }
    });
  }

  getSalesByRangedates() {
    const startDate = moment(this.searchform.value.dateStart, 'DD/MM/YYYY');
    const endDate = moment(this.searchform.value.dateEnd, 'DD/MM/YYYY');     
    this.RangeDatesFilter = [startDate, endDate];
    if (!startDate.isValid() || !endDate.isValid()) {
      this.utilityService.ShowAlert('Please insert a correct date', 'Error');
      return;    
    }
    
    if (startDate.isAfter(endDate)) {
      this.utilityService.ShowAlert('The start date must be less than the end date', 'Error');
      return;
    }

    const startDateString = moment(this.searchform.value.dateStart).format('DD/MM/YYYY');
    const endDateString = moment(this.searchform.value.dateEnd).format('DD/MM/YYYY');

    this.dashboardService.RangeDatesSummary(startDateString, endDateString).subscribe({
      next: (response) => {
        if (response.status) {
          this.totalSales = response.data.totalSales;
          this.totalRevenue = response.data.totalRevenue;
          this.totalProducts = response.data.totalProducts;
          const listData: any[] = response.data.rangeSales;
          this.topListProducts = response.data.topProducts;
          console.log('Range: ', this.topListProducts);
          const labelTemp = listData.map((item) => item.date);
          const dataTemp = listData.map((item) => item.total);

          this.showGraficLineRange(labelTemp, dataTemp);
        }
      },
      error: (error) => {
        console.error(error);
      }
    });

  }

  showGraficLineRange(graficLabel:any[], graficData:any[]){
    if ( this.LineChartRange) {
      this.LineChartRange.destroy();
    }
    this.LineChartRange = new Chart('LineChartRange', {
      type: 'line',
      data: {
        labels: graficLabel,
        datasets: [{
          label: "# Sales in Range",
          data: graficData,
          fill: false,
          borderColor: 'rgba(130, 181, 214, 1)',
          backgroundColor: 'rgba(130, 181, 214, 0.2)',
          pointStyle:'circle',
          pointRadius:10,
          pointHoverRadius:15,
          tension: 0
        }]
      },
      options: {
        plugins:{
          filler: {
            propagate: false
          },
          title:{
            display: true,
            text: 'Sales in Range',
            color: '#ccc6c6',
          }
          
        },
        responsive: true,
        maintainAspectRatio: false,
        interaction: {
          intersect: false,
          axis: 'x'
        },
        scales: {
          x: {
            grid: {
              color: 'rgba(255, 255, 255, 0.1)' // Color de las líneas de la cuadrícula en el eje X
            }
          },
          y: {
            ticks: {
              color: '#ccc6c6',
            },
            grid: {
              color: 'rgba(255, 255, 255, 0.1)' // Color de las líneas de la cuadrícula en el eje Y
            },
            min: 0 // Establecer el valor mínimo del eje Y en 0
          }
        }
      },
      
    });
  }
  

}
