import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: '',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () => import('./features/dashboard/dashboard.component').then((m) => m.DashboardComponent),
      },
      {
        path: 'admin/usuarios',
        canActivate: [roleGuard],
        data: { roles: ['Administrador'] },
        loadComponent: () =>
          import('./features/admin/user-list/user-list.component').then((m) => m.UserListComponent),
      },
      {
        path: 'configuracion',
        loadComponent: () => import('./features/settings/settings.component').then((m) => m.SettingsComponent),
      },
      {
        path: 'campanias/:campaniaId',
        loadComponent: () =>
          import('./features/campaigns/campaign-detail/campaign-detail.component').then(
            (m) => m.CampaignDetailComponent,
          ),
      },
      {
        path: 'personajes/:personajeId',
        loadComponent: () =>
          import('./features/character-sheet/character-sheet.component').then((m) => m.CharacterSheetComponent),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
