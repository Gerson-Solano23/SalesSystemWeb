import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { UtilityService } from './utility.service';

/**
 * Adds the JWT to every API call and handles authorization errors in one place:
 * 401 (expired or invalid session) logs the user out, 403 shows a "no permission" alert.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const utilityService = inject(UtilityService);
  const router = inject(Router);
  const token = utilityService.getToken();

  const isLogin = req.url.endsWith('Authenticator/Login');
  const authReq = token && !isLogin
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (!isLogin && error.status === 401) {
        utilityService.ShowAlert('Your session has expired, please log in again', 'Opps!');
        utilityService.deleteUserSession();
        router.navigate(['/login']);
      } else if (error.status === 403) {
        utilityService.ShowAlert('You do not have permission to perform this action', 'Opps!');
      } else if (error.status === 429) {
        utilityService.ShowAlert(error.error?.message ?? 'Too many requests, please try again later', 'Opps!');
      }
      return throwError(() => error);
    })
  );
};
