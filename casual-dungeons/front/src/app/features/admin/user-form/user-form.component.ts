import { ChangeDetectionStrategy, Component, computed, effect, inject, input, output, signal } from '@angular/core';
import { RolUsuario, Usuario } from '../../../core/models';
import { UsuarioService } from '../../../core/services/usuario.service';

interface EstadoFormularioUsuario {
  nombreUsuario: string;
  email: string;
  password: string;
  rol: RolUsuario;
  activo: boolean;
}

function estadoDesdeUsuario(usuario: Usuario | null): EstadoFormularioUsuario {
  if (!usuario) {
    return { nombreUsuario: '', email: '', password: '', rol: 'Jugador', activo: true };
  }
  return { nombreUsuario: usuario.nombreUsuario, email: usuario.email, password: '', rol: usuario.rol, activo: usuario.activo };
}

@Component({
  selector: 'app-user-form',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="dialog-overlay" (click)="cerrado.emit()">
      <div class="dialog user-form" (click)="$event.stopPropagation()">
        <h2 class="dialog__titulo">{{ usuario() ? 'Editar usuario' : 'Crear usuario' }}</h2>

        <div class="field">
          <label class="field__label" for="nombreUsuario">Nombre de usuario</label>
          <input
            id="nombreUsuario"
            class="input"
            type="text"
            [value]="estado().nombreUsuario"
            (input)="set('nombreUsuario', $any($event.target).value)"
          />
        </div>

        <div class="field">
          <label class="field__label" for="email">Email</label>
          <input id="email" class="input" type="email" [value]="estado().email" (input)="set('email', $any($event.target).value)" />
        </div>

        <div class="field">
          <label class="field__label" for="password">
            {{ usuario() ? 'Nueva contraseña' : 'Contraseña' }}
          </label>
          <input
            id="password"
            class="input"
            type="password"
            [value]="estado().password"
            (input)="set('password', $any($event.target).value)"
          />
          @if (usuario()) {
            <span class="user-form__ayuda">Déjalo en blanco para mantener la contraseña actual.</span>
          }
          <span class="field__error" [style.visibility]="mostrarAvisoPassword() ? 'visible' : 'hidden'">
            Mínimo 8 caracteres.
          </span>
        </div>
        <div class="field">
          <label class="field__label" for="rol">Rol</label>
          <select id="rol" class="select" [value]="estado().rol" (change)="set('rol', $any($event.target).value)">
            <option value="Jugador">Jugador</option>
            <option value="DungeonMaster">Dungeon Master</option>
            <option value="Administrador">Administrador</option>
          </select>
        </div>

        @if (usuario()) {
          <div class="field">
            <label class="field__label">
              <input type="checkbox" [checked]="estado().activo" (change)="set('activo', $any($event.target).checked)" />
              Usuario activo
            </label>
          </div>
        }

        @if (error()) {
          <p class="field__error">{{ error() }}</p>
        }

        <div class="dialog__acciones">
          <button type="button" class="btn btn--ghost" (click)="cerrado.emit()">Cancelar</button>
          <button type="button" class="btn btn--primary" [disabled]="guardando() || !formularioValido()" (click)="guardar()">
            {{ guardando() ? 'Guardando...' : 'Guardar' }}
          </button>
        </div>
      </div>
    </div>
  `,
  styleUrl: './user-form.component.scss',
})
export class UserFormComponent {
  readonly usuario = input<Usuario | null>(null);
  readonly cerrado = output<void>();
  readonly guardado = output<void>();

  private readonly usuarioService = inject(UsuarioService);

  protected readonly estado = signal<EstadoFormularioUsuario>(estadoDesdeUsuario(this.usuario()));
  protected readonly guardando = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly mostrarAvisoPassword = computed(() => {
    const password = this.estado().password;
    return password.length > 0 && password.length < 8;
  });

  protected readonly formularioValido = computed(() => {
    const valores = this.estado();
    const nombreValido = valores.nombreUsuario.trim().length > 0;
    const emailValido = valores.email.includes('@');
  const passwordValida = this.usuario()
    ? valores.password.length === 0 || valores.password.length >= 8
    : valores.password.length >= 8;
    return nombreValido && emailValido && passwordValida;
  });

  protected set<K extends keyof EstadoFormularioUsuario>(campo: K, valor: EstadoFormularioUsuario[K]): void {
    this.estado.update((actual) => ({ ...actual, [campo]: valor }));
  }

  protected guardar(): void {
    if (!this.formularioValido()) return;

    this.guardando.set(true);
    this.error.set(null);

    const valores = this.estado();
    const usuarioActual = this.usuario();

    const observable: import('rxjs').Observable<unknown> = usuarioActual
      ? this.usuarioService.editar(usuarioActual.id, {
          nombreUsuario: valores.nombreUsuario,
          email: valores.email,
          rol: valores.rol,
          activo: valores.activo,
          nuevaPassword: valores.password || null,
        })
      : this.usuarioService.crear({
          nombreUsuario: valores.nombreUsuario,
          email: valores.email,
          password: valores.password,
          rol: valores.rol,
        });

    observable.subscribe({
      next: () => {
        this.guardando.set(false);
        this.guardado.emit();
      },
      error: (err: any) => {
        this.guardando.set(false);
        this.error.set(err?.error?.error ?? 'No se pudo guardar el usuario.');
      },
    });
  }


  constructor() {
    effect(() => {
      this.estado.set(estadoDesdeUsuario(this.usuario()));
    });
  }
}
