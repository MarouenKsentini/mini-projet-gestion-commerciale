import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ToastService } from '../services/toast.service';

/**
 * Intercepte toutes les erreurs HTTP et affiche le message métier renvoyé par le back-end
 * (voir ExceptionHandlingMiddleware côté API), pour une gestion d'erreurs cohérente
 * sans dupliquer de try/catch dans chaque composant.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toast = inject(ToastService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const message = error.error?.message ?? "Une erreur est survenue. Veuillez réessayer.";
      toast.error(message);
      return throwError(() => error);
    })
  );
};
