import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { Menu } from '../../Interfaces/menu';
import { MenuService } from '../../Services/menu.service';
import { UtilityService } from '../../Reusable/utility.service';
import { LoadingService } from '../../Services/loading.service';
import { Subscription } from 'rxjs';
import { FileDownloadService } from '../../Services/file-download.service'; 
@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.css', '../../../styles.scss']
})
export class LayoutComponent {
    // En tu componente
  selectedMenuItem: string = ''; // Inicializa con la opción predeterminada
  private isDarkTheme = false;
  private subscription: Subscription;
  getControl:boolean = false;
  listMenu: Menu[] = [];
  emailUser: string = '';
  rolUser: string = '';
  fullName: string = '';
  idUserInSession: number = -1;
  constructor(
    private router: Router,
    private menuService: MenuService,
    private utilityService: UtilityService,
    private fileDownloadService: FileDownloadService,
    public loadingService: LoadingService
  ) {
    this.subscription = this.loadingService.controlExcel$.subscribe(
      (isLoading) => {
        this.getControl = isLoading;
      }
    );
   
  }

  // Método para actualizar la opción seleccionada
  selectMenuItem(menuItem: string) {
    this.utilityService.checkTokendExpired();
    if (menuItem != 'Reports' &&  this.getControl == true) {
      this.loadingService.inactive();
     
      this.fileDownloadService.freeStorage(this.idUserInSession).subscribe({
        next:(response)=>{
          console.log(response);
        }
      });     
    }
    this.selectedMenuItem = menuItem;
    if(this.selectedMenuItem == null || this.selectedMenuItem == '') {
      this.selectedMenuItem = '';
    }
  }
  
  ngOnInit():void{
    
    const user = this.utilityService.getUserSession();
    if (user != null) {
      this.idUserInSession = user.idUser;
      this.fullName = user.fullName;
      this.emailUser = user.email;
      this.rolUser = user.rolDescription;

      this.menuService.listMenus(this.idUserInSession).subscribe({
        next: (response) => {
          if (response.status) {
            this.listMenu = response.data as Menu[];
          }
        },
        error: (error) => {
          console.error(error);
        }
      });

    }else{
      this.router.navigate(['/login']);
    }
  }

  logout():void{
    this.utilityService.deleteUserSession();
    this.router.navigate(['/login']);
    
  }


}
