import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormField, form, required, submit } from '@angular/forms/signals';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../../core/services/auth.service';
import { ThemeService } from '../../../core/services/theme.service';
import { CornerFrameComponent } from '../../../shared/components/corner-frame/corner-frame.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormField, CornerFrameComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="login-page">
      <app-corner-frame>
        <form class="login-card" (submit)="onSubmit($event)">
          <h1 class="login-card__titulo">Casual Dungeons</h1>
          <p class="login-card__subtitulo">Inicia sesión para continuar tu aventura</p>

          <div class="field">
            <label class="field__label" for="nombreUsuario">Nombre de usuario</label>
            <input
              id="nombreUsuario"
              class="input"
              [class.input--invalid]="credencialesForm.nombreUsuario().touched() && !credencialesForm.nombreUsuario().valid()"
              type="text"
              autocomplete="username"
              [formField]="credencialesForm.nombreUsuario"
            />
            @if (credencialesForm.nombreUsuario().touched() && !credencialesForm.nombreUsuario().valid()) {
              <span class="field__error">El nombre de usuario es obligatorio.</span>
            }
          </div>

          <div class="field">
            <label class="field__label" for="password">Contraseña</label>
            <input
              id="password"
              class="input"
              [class.input--invalid]="credencialesForm.password().touched() && !credencialesForm.password().valid()"
              type="password"
              autocomplete="current-password"
              [formField]="credencialesForm.password"
            />
            @if (credencialesForm.password().touched() && !credencialesForm.password().valid()) {
              <span class="field__error">La contraseña es obligatoria.</span>
            }
          </div>

          @if (errorServidor()) {
            <p class="login-card__error">{{ errorServidor() }}</p>
          }

          <button type="submit" class="btn btn--primary login-card__submit" [disabled]="enviando()">
            {{ enviando() ? 'Entrando...' : 'Entrar' }}
          </button>
        </form>
      </app-corner-frame>
    </div>
  `,
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly themeService = inject(ThemeService);

  protected readonly enviando = signal(false);
  protected readonly errorServidor = signal<string | null>(null);

  private readonly credenciales = signal({ nombreUsuario: '', password: '' });

  protected readonly credencialesForm = form(this.credenciales, (path) => {
    required(path.nombreUsuario, { message: 'El nombre de usuario es obligatorio.' });
    required(path.password, { message: 'La contraseña es obligatoria.' });
  });

  protected onSubmit(event: SubmitEvent): void {
    event.preventDefault();
    this.errorServidor.set(null);

    submit(this.credencialesForm, async (form) => {
      this.enviando.set(true);

      try {
        await new Promise<void>((resolve, reject) => {
          this.authService.login(form().value()).subscribe({
            next: () => resolve(),
            error: (err: HttpErrorResponse) => reject(err),
          });
        });

        this.themeService.cargarDesdeServidor();

        const destino = this.authService.esAdministrador() ? '/admin/usuarios' : '/';
        this.router.navigate([destino]);
        return undefined;
      } catch (err) {
        const mensaje =
          err instanceof HttpErrorResponse && err.error?.error
            ? (err.error.error as string)
            : 'No se pudo iniciar sesión. Intenta de nuevo.';
        this.errorServidor.set(mensaje);
        return [{ kind: 'server', message: mensaje }];
      } finally {
        this.enviando.set(false);
      }
    });
  }
}
