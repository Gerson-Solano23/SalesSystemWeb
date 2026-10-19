import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import {
  Component,
  ElementRef,
  OnInit,
  Renderer2,
  ChangeDetectionStrategy,
} from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UploadFileService } from '../../../../Services/upload-file.service';
import { LoadingService } from '../../../../Services/loading.service';
import { UtilityService } from '../../../../Reusable/utility.service';

@Component({
  selector: 'app-files',
  templateUrl: './files.component.html',
  styleUrls: ['./files.component.css', '../../../../../styles.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false,
})
export class FilesComponent implements OnInit {
  // Same extensions and size limit the API accepts.
  readonly acceptedExtensions = '.pdf,.txt,.csv,.doc,.docx,.xls,.xlsx,.png,.jpg,.jpeg';
  private readonly maxFileSizeBytes = 10 * 1024 * 1024;

  selectedFile: File | null = null;
  uploading = false;

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
      case 'csv':
        return 'CSV File';
      case 'doc':
      case 'docx':
        return 'Word Document';
      case 'xls':
      case 'xlsx':
        return 'Excel Spreadsheet';
      case 'png':
      case 'jpg':
      case 'jpeg':
        return 'Image';
      default:
        return 'Unknown File Type';
    }
  }

  // Icon shown for a stored file in the History Reports panel.
  fileIcon(fileName: string): string {
    const extension = fileName.split('.').pop()?.toLowerCase();
    switch (extension) {
      case 'pdf':
        return '/pdfIcon.png';
      case 'txt':
      case 'csv':
        return '/txtIcon.png';
      case 'doc':
      case 'docx':
        return '/documentowordIcon.png';
      default:
        return '/xlsxIcon.png';
    }
  }

  onSubmit(fileInput: HTMLInputElement) {
    const file = this.selectedFile;
    if (!file || this.uploading) {
      return;
    }
    if (file.size > this.maxFileSizeBytes) {
      this.utilityService.ShowAlert('The file exceeds the 10 MB limit', 'Error');
      return;
    }

    this.uploading = true;
    this.uploadfile.uploadFile(file).subscribe({
      next: (response) => {
        this.uploading = false;
        if (response.status) {
          this.utilityService.ShowAlert(`${file.name} uploaded. You will find it in History Reports, in the current month.`, 'Success');
          this.selectedFile = null;
          fileInput.value = '';
          this.loadYears();
        } else {
          this.utilityService.ShowAlert(response.message ?? 'The file could not be uploaded', 'Error');
        }
      },
      error: (error) => {
        this.uploading = false;
        this.utilityService.ShowAlert(error.error?.message ?? 'The file could not be uploaded', 'Error');
      },
    });
  }
  formUser: FormGroup;
  isXsScreen: boolean = false;
  isSmScreen: boolean = false;
  isMdScreen: boolean = false;
  isLgScreen: boolean = false;
  yearChoose: string = '';
  MonthChoose: string = '';
  // Loaded from the API: years with stored files plus the current year.
  years: { value: string; viewValue: string }[] = [];

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
  ];

  excelFiles: any = [];

  constructor(
    private breakpointObserver: BreakpointObserver,
    private renderer: Renderer2,
    private form: FormBuilder,
    private el: ElementRef,
    private uploadfile: UploadFileService,
    private loadingService: LoadingService,
    private utilityService: UtilityService,
  ) {
    this.formUser = this.form.group({
      FullName: ['', Validators.required],
      File: ['', Validators.required],
    });
  }

  ngOnInit(): void {
    this.breakpointObserver
      .observe([
        Breakpoints.XSmall,
        Breakpoints.Small,
        Breakpoints.Medium,
        Breakpoints.Large,
      ])
      .subscribe((result) => {
        this.isXsScreen = result.breakpoints[Breakpoints.XSmall];
        this.isSmScreen = result.breakpoints[Breakpoints.Small];
        this.isMdScreen = result.breakpoints[Breakpoints.Medium];
        this.isLgScreen = result.breakpoints[Breakpoints.Large];
      });
    this.loadYears();
  }

  loadYears(): void {
    this.uploadfile.getYears().subscribe({
      next: (response) => {
        const years: number[] = response.status ? response.data : [];
        this.years = (years.length ? years : [new Date().getFullYear()]).map(
          (year) => ({ value: String(year), viewValue: String(year) }),
        );
      },
      error: () => {
        const year = String(new Date().getFullYear());
        this.years = [{ value: year, viewValue: year }];
      },
    });
  }

  ngAfterContentInit(): void {
    const elemntTree = this.renderer.selectRootElement('#treeMatGrid', true);
    this.renderer.setStyle(elemntTree, 'margin-top', 'calc(26% + 0.5px) ');
  }

  goBackMainPanel() {
    const elementfileExcels = this.renderer.selectRootElement(
      '#fileExcels',
      true,
    );
    if (!elementfileExcels.classList.contains('hide')) {
      this.renderer.addClass(elementfileExcels, 'hide');
    }
    const elemntOne = this.renderer.selectRootElement('#oneMatGrid', true);
    const elemntTree = this.renderer.selectRootElement('#treeMatGrid', true);
    const buttonGoBack = this.renderer.selectRootElement(
      '#goBackIconButton',
      true,
    );
    const elementFilesMonthly = this.renderer.selectRootElement(
      '#fileMonthly',
      true,
    );
    this.renderer.addClass(buttonGoBack, 'hide');
    this.renderer.removeClass(elemntOne, 'hide');
    this.renderer.setStyle(elemntOne, 'height', '100%');
    this.renderer.removeClass(elemntTree, 'hide');
    this.renderer.addClass(elementFilesMonthly, 'hide');
    this.renderer.removeClass(elementFilesMonthly, 'height');
    this.renderer.setStyle(
      elementFilesMonthly,
      'max-width',
      '1000px !important',
    );
    this.renderer.removeClass(elemntOne, 'custom-height');
    this.renderer.removeStyle(elemntOne, 'max-width');
  }

  showExcelReportPerMonth(month: string) {
    const normalizedMonthName = month.toLowerCase();
    var foundMonth = this.months.find(
      (month) => month.viewValue.toLowerCase() === normalizedMonthName,
    )?.value;
    if (!foundMonth) {
      foundMonth = '';
    }
    this.getfilesPerMonth(foundMonth, this.yearChoose);

    this.MonthChoose = month;
    const elementFilesMonthly = this.renderer.selectRootElement(
      '#fileMonthly',
      true,
    );
    this.renderer.addClass(elementFilesMonthly, 'hide');
    const elementfileExcels = this.renderer.selectRootElement(
      '#fileExcels',
      true,
    );
    this.renderer.removeClass(elementfileExcels, 'hide');
    this.renderer.removeStyle(elementfileExcels, 'margin-top');
    //this.renderer.setStyle(elementfileExcels, 'margin-top', '0px !important');
    this.renderer.setStyle(elementfileExcels, 'max-width', '75% !important');
    this.renderer.setStyle(elementfileExcels, 'left', '100% !important');
    this.renderer.setStyle(elementfileExcels, 'width', '75% !important');
    this.renderer.setStyle(elementfileExcels, 'justify-content', 'start');
    this.renderer.setStyle(elementfileExcels, 'height', '83vh !important');
  }

  hideOthersLeesOne(idElement: string, event: any) {
    const elementfileExcels = this.renderer.selectRootElement(
      '#fileExcels',
      true,
    );
    if (!elementfileExcels.classList.contains('hide')) {
      this.renderer.addClass(elementfileExcels, 'hide');
    }

    this.yearChoose = event;
    const elemntOne = this.renderer.selectRootElement('#oneMatGrid', true);
    const elemntTree = this.renderer.selectRootElement('#treeMatGrid', true);

    if (idElement === 'oneMatGrid') {
      const buttonGoBack = this.renderer.selectRootElement(
        '#goBackIconButton',
        true,
      );
      this.renderer.removeClass(buttonGoBack, 'hide');
      const elementFilesMonthly = this.renderer.selectRootElement(
        '#fileMonthly',
        true,
      );
      this.renderer.addClass(elemntTree, 'hide');
      this.renderer.removeClass(elementFilesMonthly, 'hide');
      this.renderer.removeClass(elementFilesMonthly, 'height');
      this.renderer.setStyle(
        elementFilesMonthly,
        'max-width',
        '25% !important',
      );

      this.renderer.addClass(elemntOne, 'custom-height');

      // Ajustar estilos para el elemento
      const elemntOne_GridTitle = this.renderer.selectRootElement(
        '#oneMatGridCard',
        true,
      );

      this.renderer.setStyle(elemntOne, 'height', '85vh');
      this.renderer.setStyle(elemntOne, 'max-width', '25%');
      this.renderer.setStyle(elementFilesMonthly, 'width', '75%');
      this.renderer.setStyle(elementFilesMonthly, 'justify-content', 'start');
      this.renderer.setStyle(elementFilesMonthly, 'height', '100%');
    } else if (idElement === 'treeMatGrid') {
      this.renderer.addClass(elemntOne, 'hide');
    }
  }

  getfilesPerMonth(month: string, year: string) {
    this.uploadfile.getListPerMonth(month, year).subscribe({
      next: (response) => {
        if (response.status) {
          const result = response.data;
          this.excelFiles = result;
        }
      },
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
          // Generic type: the panel holds Excel reports and uploaded documents.
          const blob = new Blob([byteArray], { type: 'application/octet-stream' });

          // Crear un enlace de descarga y hacer clic en él
          const link = document.createElement('a');
          link.href = window.URL.createObjectURL(blob);

          link.download = fileName;
          link.click();
          window.URL.revokeObjectURL(link.href); // Limpieza del objeto URL
          this.loadingService.hide();
        }
      },
      error: (error) => {
        this.utilityService.ShowAlert('Error downloading the file', 'Error');
        this.loadingService.hide();
      },
    });
  }
}
