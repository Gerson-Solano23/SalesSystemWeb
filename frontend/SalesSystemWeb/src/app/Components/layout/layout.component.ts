import { Component } from '@angular/core';

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.css'
})
export class LayoutComponent {
    // En tu componente
  selectedMenuItem: string = ''; // Inicializa con la opción predeterminada
  // Método para actualizar la opción seleccionada
  selectMenuItem(menuItem: string) {
    this.selectedMenuItem = menuItem;
    if(this.selectedMenuItem == null || this.selectedMenuItem == '') {
      this.selectedMenuItem = '';
    }
  }

}
