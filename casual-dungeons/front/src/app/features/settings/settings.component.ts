import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';
import { ThemeService } from '../../core/services/theme.service';

@Component({
  selector: 'app-settings',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-container settings">
      <h1>Configuración</h1>

      <section class="settings__seccion">
        <h2>Apariencia</h2>
        <div class="settings__tema">
          <button
            type="button"
            class="btn"
            [class.btn--primary]="themeService.esOscuro()"
            [class.btn--ghost]="!themeService.esOscuro()"
            (click)="themeService.establecer('dark')"
          >
            🌙 Modo oscuro
          </button>
          <button
            type="button"
            class="btn"
            [class.btn--primary]="!themeService.esOscuro()"
            [class.btn--ghost]="themeService.esOscuro()"
            (click)="themeService.establecer('light')"
          >
            ☀ Modo claro
          </button>
        </div>
      </section>

      <section class="settings__seccion">
        <h2>Nombre de usuario</h2>
        <div class="field">
          <input class="input" type="text" [value]="nuevoNombreUsuario()" (input)="nuevoNombreUsuario.set($any($event.target).value)" />
        </div>
        @if (mensajeNombreUsuario()) {
          <p [class]="errorNombreUsuario() ? 'field__error' : 'settings__exito'">{{ mensajeNombreUsuario() }}</p>
        }
        <button type="button" class="btn btn--primary" [disabled]="guardandoNombreUsuario()" (click)="guardarNombreUsuario()">
          {{ guardandoNombreUsuario() ? 'Guardando...' : 'Guardar nombre de usuario' }}
        </button>
      </section>

      <section class="settings__seccion">
        <h2>Cambiar contraseña</h2>
        <div class="field">
          <label class="field__label" for="passwordActual">Contraseña actual</label>
          <input
            id="passwordActual"
            class="input"
            type="password"
            [value]="passwordActual()"
            (input)="passwordActual.set($any($event.target).value)"
          />
        </div>
        <div class="field">
          <label class="field__label" for="passwordNueva">Nueva contraseña</label>
          <input
            id="passwordNueva"
            class="input"
            type="password"
            [value]="passwordNueva()"
            (input)="passwordNueva.set($any($event.target).value)"
          />
        </div>
        @if (mensajePassword()) {
          <p [class]="errorPassword() ? 'field__error' : 'settings__exito'">{{ mensajePassword() }}</p>
        }
        <button type="button" class="btn btn--primary" [disabled]="guardandoPassword() || !passwordValida()" (click)="guardarPassword()">
          {{ guardandoPassword() ? 'Guardando...' : 'Cambiar contraseña' }}
        </button>
      </section>
    </div>
  `,
  styleUrl: './settings.component.scss',
})
export class SettingsComponent {
  protected readonly authService = inject(AuthService);
  protected readonly themeService = inject(ThemeService);

  protected readonly nuevoNombreUsuario = signal(this.authService.nombreUsuario() ?? '');
  protected readonly guardandoNombreUsuario = signal(false);
  protected readonly mensajeNombreUsuario = signal<string | null>(null);
  protected readonly errorNombreUsuario = signal(false);

  protected readonly passwordActual = signal('');
  protected readonly passwordNueva = signal('');
  protected readonly guardandoPassword = signal(false);
  protected readonly mensajePassword = signal<string | null>(null);
  protected readonly errorPassword = signal(false);

  protected readonly passwordValida = computed(
    () => this.passwordActual().length > 0 && this.passwordNueva().length >= 8,
  );

  protected guardarNombreUsuario(): void {
    const nuevoNombre = this.nuevoNombreUsuario().trim();
    if (!nuevoNombre) return;

    this.guardandoNombreUsuario.set(true);
    this.mensajeNombreUsuario.set(null);

    this.authService.cambiarNombreUsuario({ nuevoNombreUsuario: nuevoNombre }).subscribe({
      next: () => {
        this.guardandoNombreUsuario.set(false);
        this.errorNombreUsuario.set(false);
        this.mensajeNombreUsuario.set('Nombre de usuario actualizado.');
      },
      error: (err) => {
        this.guardandoNombreUsuario.set(false);
        this.errorNombreUsuario.set(true);
        this.mensajeNombreUsuario.set(err?.error?.error ?? 'No se pudo actualizar el nombre de usuario.');
      },
    });
  }

  protected guardarPassword(): void {
    if (!this.passwordValida()) return;

    this.guardandoPassword.set(true);
    this.mensajePassword.set(null);

    this.authService
      .cambiarPassword({ passwordActual: this.passwordActual(), passwordNueva: this.passwordNueva() })
      .subscribe({
        next: () => {
          this.guardandoPassword.set(false);
          this.errorPassword.set(false);
          this.mensajePassword.set('Contraseña actualizada.');
          this.passwordActual.set('');
          this.passwordNueva.set('');
        },
        error: (err) => {
          this.guardandoPassword.set(false);
          this.errorPassword.set(true);
          this.mensajePassword.set(err?.error?.error ?? 'No se pudo actualizar la contraseña.');
        },
      });
  }
}
