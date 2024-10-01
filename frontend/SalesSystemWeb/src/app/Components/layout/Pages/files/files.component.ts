import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Component, ElementRef, OnInit, Renderer2 } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UploadFileService } from '../../../../Services/upload-file.service';
import { LoadingService } from '../../../../Services/loading.service';
import { UtilityService } from '../../../../Reusable/utility.service';





@Component({
  selector: 'app-files',
  templateUrl: './files.component.html',
  styleUrls: ['./files.component.css', '../../../../../styles.scss'],
})

export class FilesComponent implements OnInit {


  selectedFile: File | null = null;

  onFileSelected(event: any) {
    const file: File = event.target.files[0];
   
      this.selectedFile = file;
  }
  getFileType(file: File): string {
    const extension = file.name.split('.').pop()?.toLowerCase();
    switch (extension) {
      case 'pdf':
        return 'PDF Document';
      case 'txt':
        return 'Text File';
      case 'docx':
        return 'Word Document';
      case 'xlsx':
        return 'Excel Spreadsheet';
      default:
        return 'Unknown File Type';
    }
  }
  onSubmit() {
    if (this.selectedFile) {
      const formData = new FormData();
      formData.append('pdfFile', this.selectedFile, this.selectedFile.name);

      console.log('Form submitted', formData);
      // Here you would typically send the formData to your backend
      // using an HTTP service, for example:
      // this.http.post('your-api-endpoint', formData).subscribe(...)
    }
  }
  formUser: FormGroup;
  isXsScreen: boolean = false;
  isSmScreen: boolean = false;
  isMdScreen: boolean = false;
  isLgScreen: boolean = false;
  yearChoose: string = '';
  MonthChoose: string = '';
  years = [
    { value: '2020', viewValue: '2020' },
    { value: '2021', viewValue: '2021' },
    { value: '2022', viewValue: '2022' },
    { value: '2023', viewValue: '2023' },
    { value: '2024', viewValue: '2024' }
  ];

  months = [
    { value: '1', viewValue: 'Enero' },
    { value: '2', viewValue: 'Febrero' },
    { value: '3', viewValue: 'Marzo' },
    { value: '4', viewValue: 'Abril' },
    { value: '5', viewValue: 'Mayo' },
    { value: '6', viewValue: 'Junio' },
    { value: '7', viewValue: 'Julio' },
    { value: '8', viewValue: 'Agosto' },
    { value: '9', viewValue: 'Septiembre' },
    { value: '10', viewValue: 'Octubre' },
    { value: '11', viewValue: 'Noviembre' },
    { value: '12', viewValue: 'Diciembre' },
  ]


  excelFiles: any = []



  constructor(
    private breakpointObserver: BreakpointObserver,
    private renderer: Renderer2,
    private form: FormBuilder,
    private el: ElementRef,
    private uploadfile: UploadFileService,
    private loadingService: LoadingService,
    private utilityService: UtilityService
  ) {

    this.formUser = this.form.group({
      FullName: ['', Validators.required],
      File: ['', Validators.required],

    });
  }

  ngOnInit(): void {
    this.breakpointObserver.observe([Breakpoints.XSmall, Breakpoints.Small, Breakpoints.Medium, Breakpoints.Large])
      .subscribe(result => {
        this.isXsScreen = result.breakpoints[Breakpoints.XSmall];
        this.isSmScreen = result.breakpoints[Breakpoints.Small];
        this.isMdScreen = result.breakpoints[Breakpoints.Medium];
        this.isLgScreen = result.breakpoints[Breakpoints.Large];
      });

  }

  ngAfterContentInit(): void {
    const elemntTree = this.renderer.selectRootElement('#treeMatGrid', true);
    const elemntFour = this.renderer.selectRootElement('#fourMatGrid', true);
    this.renderer.setStyle(elemntTree, 'margin-top', 'calc(26% + 0.5px) ');
    this.renderer.setStyle(elemntFour, 'margin-top', 'calc(26% + 0.5px)');
  }

  goBackMainPanel() {
    const elementfileExcels = this.renderer.selectRootElement('#fileExcels', true);
    if (!elementfileExcels.classList.contains('hide')) {
      this.renderer.addClass(elementfileExcels, 'hide');
    }
    const elemntOne = this.renderer.selectRootElement('#oneMatGrid', true);
    const elemntTwo = this.renderer.selectRootElement('#twoMatGrid', true);
    const elemntTree = this.renderer.selectRootElement('#treeMatGrid', true);
    const elemntFour = this.renderer.selectRootElement('#fourMatGrid', true);
    const buttonGoBack = this.renderer.selectRootElement('#goBackIconButton', true);
    const elementFilesMonthly = this.renderer.selectRootElement('#fileMonthly', true);
    this.renderer.addClass(buttonGoBack, 'hide');
    this.renderer.removeClass(elemntOne, 'hide');
    this.renderer.setStyle(elemntOne, 'height', '100%');
    this.renderer.removeClass(elemntTwo, 'hide');
    this.renderer.removeClass(elemntTree, 'hide');
    this.renderer.removeClass(elemntFour, 'hide');
    this.renderer.addClass(elementFilesMonthly, 'hide');
    this.renderer.removeClass(elementFilesMonthly, 'height');
    this.renderer.setStyle(elementFilesMonthly, 'max-width', '1000px !important');
    this.renderer.removeClass(elemntOne, 'custom-height');
    this.renderer.removeStyle(elemntOne, 'max-width');
  }


  showExcelReportPerMonth(month: string) {
    const normalizedMonthName = month.toLowerCase();
    var foundMonth = this.months.find((month) => month.viewValue.toLowerCase() === normalizedMonthName)?.value;
    if (!foundMonth) {
      foundMonth = '';
    }
    this.getfilesPerMonth(foundMonth, this.yearChoose);

    console.log('Month Choose', this.yearChoose, month);
    this.MonthChoose = month;
    const elementFilesMonthly = this.renderer.selectRootElement('#fileMonthly', true);
    this.renderer.addClass(elementFilesMonthly, 'hide');
    const elementfileExcels = this.renderer.selectRootElement('#fileExcels', true);
    this.renderer.removeClass(elementfileExcels, 'hide');
    this.renderer.removeStyle(elementfileExcels, 'margin-top');
    //this.renderer.setStyle(elementfileExcels, 'margin-top', '0px !important');
    this.renderer.setStyle(elementfileExcels, 'max-width', '75% !important');
    this.renderer.setStyle(elementfileExcels, 'left', '100% !important');
    this.renderer.setStyle(elementfileExcels, 'width', '75% !important');
    this.renderer.setStyle(elementfileExcels, 'justify-content', 'start');
    this.renderer.setStyle(elementfileExcels, 'height', '83vh !important');

    console.log('Month Choose', this.MonthChoose);
  }

  hideOthersLeesOne(idElement: string, event: any) {
    const elementfileExcels = this.renderer.selectRootElement('#fileExcels', true);
    if (!elementfileExcels.classList.contains('hide')) {
      this.renderer.addClass(elementfileExcels, 'hide');
    }

    this.yearChoose = event;
    console.log('HIDE FUNTION IS RUNNING', idElement, event, this.yearChoose);
    const elemntOne = this.renderer.selectRootElement('#oneMatGrid', true);
    const elemntTwo = this.renderer.selectRootElement('#twoMatGrid', true);
    const elemntTree = this.renderer.selectRootElement('#treeMatGrid', true);
    const elemntFour = this.renderer.selectRootElement('#fourMatGrid', true);

    if (idElement === 'oneMatGrid') {
      const buttonGoBack = this.renderer.selectRootElement('#goBackIconButton', true);
      this.renderer.removeClass(buttonGoBack, 'hide');
      const elementFilesMonthly = this.renderer.selectRootElement('#fileMonthly', true);
      this.renderer.addClass(elemntTwo, 'hide');
      this.renderer.addClass(elemntTree, 'hide');
      this.renderer.addClass(elemntFour, 'hide');
      this.renderer.removeClass(elementFilesMonthly, 'hide');
      this.renderer.removeClass(elementFilesMonthly, 'height');
      this.renderer.setStyle(elementFilesMonthly, 'max-width', '25% !important');

      this.renderer.addClass(elemntOne, 'custom-height');



      // Ajustar estilos para el elemento
      const elemntOne_GridTitle = this.renderer.selectRootElement('#oneMatGridCard', true);

      this.renderer.setStyle(elemntOne, 'height', '85vh');
      this.renderer.setStyle(elemntOne, 'max-width', '25%');
      this.renderer.setStyle(elementFilesMonthly, 'width', '75%');
      this.renderer.setStyle(elementFilesMonthly, 'justify-content', 'start');
      this.renderer.setStyle(elementFilesMonthly, 'height', '100%');

    } else if (idElement === 'twoMatGrid') {

      this.renderer.addClass(elemntOne, 'hide');
      this.renderer.addClass(elemntTree, 'hide');
      this.renderer.addClass(elemntFour, 'hide');

    } else if (idElement === 'treeMatGrid') {

      this.renderer.addClass(elemntOne, 'hide');
      this.renderer.addClass(elemntTwo, 'hide');
      this.renderer.addClass(elemntFour, 'hide');

    } else if (idElement === 'fourMatGrid') {

      this.renderer.addClass(elemntOne, 'hide');
      this.renderer.addClass(elemntTwo, 'hide');
      this.renderer.addClass(elemntTree, 'hide');

    }


  }

  onFileSelected2(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      console.log(file.name);
      // Aquí puedes realizar otras operaciones con el archivo seleccionado
    }
  }

  getfilesPerMonth(month: string, year: string) {
    this.uploadfile.getListPerMonth(month, year).subscribe({
      next: (response) => {
        if (response.status) {
          const result = response.data;
          this.excelFiles = result;
          console.log('Response getfilesPerMonth', result);
        }
      }
    });
  }

  downloadExcelFile(fileName: string) {
    this.loadingService.show();

    this.uploadfile.DownloadFile(fileName).subscribe({
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

          link.download = fileName;
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
}
