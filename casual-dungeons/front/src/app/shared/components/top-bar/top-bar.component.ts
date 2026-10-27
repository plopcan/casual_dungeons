import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ThemeService } from '../../../core/services/theme.service';

@Component({
  selector: 'app-top-bar',
  standalone: true,
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <header class="top-bar">
      <a routerLink="/" class="top-bar__brand">
        <span class="top-bar__brand-icon" aria-hidden="true">⚔</span>
        <span class="top-bar__brand-text">Casual Dungeons</span>
      </a>

      <div class="top-bar__usuario">
        @if (authService.nombreUsuario(); as nombre) {
          <span class="top-bar__nombre">{{ nombre }}</span>
          <span class="badge badge--gold">{{ authService.rol() }}</span>
        }
      </div>

      <div class="top-bar__acciones">
        @if (authService.esAdministrador()) {
          <a routerLink="/admin/usuarios" class="btn btn--ghost btn--sm">Usuarios</a>
        }

        <button
          type="button"
          class="btn btn--ghost btn--icon"
          [attr.aria-label]="themeService.esOscuro() ? 'Cambiar a modo claro' : 'Cambiar a modo oscuro'"
          (click)="themeService.alternar()"
        >
          {{ themeService.esOscuro() ? '☀' : '🌙' }}
        </button>

        <a routerLink="/configuracion" class="btn btn--ghost btn--icon" aria-label="Configuración"> ⚙ </a>

        <button type="button" class="btn btn--ghost" (click)="cerrarSesion()">Salir</button>
      </div>
    </header>
  `,
  styleUrl: './top-bar.component.scss',
})
export class TopBarComponent {
  protected readonly authService = inject(AuthService);
  protected readonly themeService = inject(ThemeService);
  private readonly router = inject(Router);

  cerrarSesion(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
