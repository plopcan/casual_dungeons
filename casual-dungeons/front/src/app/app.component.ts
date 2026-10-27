import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from './core/services/auth.service';
import { ThemeService } from './core/services/theme.service';
import { ConfirmDialogComponent } from './shared/components/confirm-dialog/confirm-dialog.component';
import { TopBarComponent } from './shared/components/top-bar/top-bar.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, TopBarComponent, ConfirmDialogComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (authService.estaAutenticado()) {
      <app-top-bar></app-top-bar>
    }

    <main>
      <router-outlet></router-outlet>
    </main>

    <app-confirm-dialog></app-confirm-dialog>
  `,
})
export class AppComponent {
  protected readonly authService = inject(AuthService);

  // Se inyecta para forzar la inicialización temprana del servicio
  // (aplica el tema oscuro por defecto al <html> desde el arranque,
  // antes de que el usuario inicie sesión).
  private readonly themeService = inject(ThemeService);
}
