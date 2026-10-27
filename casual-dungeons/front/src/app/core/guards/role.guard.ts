import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { RolUsuario } from '../models';

/**
 * Restringe el acceso a una ruta según el rol del usuario. Se
 * configura en la definición de ruta mediante `data: { roles: [...] }`.
 *
 * Ejemplo:
 *   { path: 'admin', canActivate: [roleGuard], data: { roles: ['Administrador'] } }
 *
 * Si el usuario no tiene sesión, delega en authGuard (debe combinarse
 * con él en canActivate). Si tiene sesión pero no el rol requerido,
 * redirige al dashboard.
 */
export const roleGuard: CanActivateFn = (route) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const rolesPermitidos = route.data['roles'] as RolUsuario[] | undefined;

  if (!rolesPermitidos || rolesPermitidos.length === 0) {
    return true;
  }

  const rolActual = authService.rol();

  if (rolActual && rolesPermitidos.includes(rolActual)) {
    return true;
  }

  return router.createUrlTree(['/']);
};
