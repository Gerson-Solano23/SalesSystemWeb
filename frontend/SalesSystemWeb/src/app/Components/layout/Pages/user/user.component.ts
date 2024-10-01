import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';

import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatDialog } from '@angular/material/dialog';
import { ModalUserComponent } from '../../Modals/modal-user/modal-user.component';
import { User } from '../../../../Interfaces/user';
import { UserService } from '../../../../Services/user.service';
import { UtilityService } from '../../../../Reusable/utility.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-user',
  templateUrl: './user.component.html',
  styleUrls: ['./user.component.css', '../../../../../styles.scss']
})
export class UserComponent implements OnInit, AfterViewInit {

  columsTable: string[] = ['Consecutivo', 'FullName', 'Email', 'RolDescription', 'Status', 'Actions'];
  dataUsers: User[] = [];  
  dataListusers = new MatTableDataSource(this.dataUsers);
  pageIndex: number = 0;
  pageSize: number = 10;
  
  @ViewChild(MatPaginator) paginator!: MatPaginator;  

  constructor(
    private dialog: MatDialog,
    private userService: UserService,
    private utilityService: UtilityService
    
  ) { }
  getListUsers() {
    this.userService.listUsers().subscribe({
      next: (response) => {
        if (response.status) {
          this.dataUsers = response.data;
          this.dataListusers.data = this.dataUsers;
          this.dataListusers.paginator = this.paginator;
        }else{
          this.utilityService.ShowAlert("Users not found", 'Error');
        }
      },
      error: (error) => {}
    });  
  }
  ngOnInit(): void {
    this.utilityService.onlyRolOne();
    this.getListUsers();
  }

  ngAfterViewInit(): void {
    this.dataListusers.paginator = this.paginator;
  }

  filterResearh(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataListusers.filter = filterValue.trim().toLowerCase();
  }

  createUser(){
    this.dialog.open(ModalUserComponent, {
      disableClose: true
    }).afterClosed().subscribe(result=>{
      if (result === "true") {
        this.getListUsers();     
      }
    });
  }

  updateUser(user: User){
    this.dialog.open(ModalUserComponent, {
      disableClose: true,
      data: user
    }).afterClosed().subscribe(result=>{
      if (result === "true") {
        this.getListUsers();     
      }
    });
  }
  deleteUser(user: User){
    Swal.fire({
      title:'Are you sure to remove this user?',
      text: user.fullName,
      icon: 'warning',
      confirmButtonColor: '#3085d6',
      confirmButtonText: 'Yes, remove it!',
      showCancelButton: true,
      cancelButtonColor: '#d33',
      cancelButtonText: 'Cancel'
    }).then((result)=>{
      if (result.isConfirmed) {
        this.userService.deleteUser(user.idUser).subscribe({
          next:(resultData)=>{
            console.log('resultData', resultData);
            if (resultData.status) {
              this.utilityService.ShowAlert('User was successfully removed', 'success');
              this.getListUsers();
              this.paginator.firstPage();
            }else{
              this.utilityService.ShowAlert('User could not be removed', 'error');
            }
          },
          error:(e)=>{}
        })
      }
    })
  }
  
  onPageChange(event: PageEvent) {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
  }

  getConsecutive(index: number): number {
    return index + this.pageIndex * this.pageSize + 1;
  }
}
