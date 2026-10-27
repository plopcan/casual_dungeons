import { httpResource } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { environment } from '../../../../environments/environment';
import { Usuario } from '../../../core/models';
import { UsuarioService } from '../../../core/services/usuario.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog.service';
import { UserFormComponent } from '../user-form/user-form.component';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [UserFormComponent, DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-container">
      <div class="user-list__header">
        <h1>Administración de usuarios</h1>
        <button type="button" class="btn btn--primary" (click)="abrirCreacion()">+ Crear usuario</button>
      </div>

      @if (usuariosResource.isLoading()) {
        <p class="empty-state">Cargando usuarios...</p>
      } @else {
        <table class="user-list__tabla">
          <thead>
            <tr>
              <th>Usuario</th>
              <th>Email</th>
              <th>Rol</th>
              <th>Estado</th>
              <th>Creado</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            @for (usuario of usuariosResource.value(); track usuario.id) {
              <tr>
                <td>{{ usuario.nombreUsuario }}</td>
                <td>{{ usuario.email }}</td>
                <td><span class="badge badge--gold">{{ usuario.rol }}</span></td>
                <td>
                  @if (usuario.activo) {
                    <span class="badge badge--success">Activo</span>
                  } @else {
                    <span class="badge badge--muted">Inactivo</span>
                  }
                </td>
                <td>{{ usuario.fechaCreacion | date: 'dd/MM/yyyy' }}</td>
                <td class="user-list__acciones">
                  <button type="button" class="btn btn--ghost btn--sm" (click)="abrirEdicion(usuario)">Editar</button>
                  @if (usuario.activo) {
                    <button type="button" class="btn btn--danger btn--sm" (click)="confirmarDesactivacion(usuario)">Desactivar</button>
                  }
                </td>
              </tr>
            }
          </tbody>
        </table>
      }
    </div>

    @if (mostrarFormulario()) {
      <app-user-form
        [usuario]="usuarioEditando()"
        (cerrado)="cerrarFormulario()"
        (guardado)="onGuardado()"
      ></app-user-form>
    }
  `,
  styleUrl: './user-list.component.scss',
})
export class UserListComponent {
  private readonly usuarioService = inject(UsuarioService);
  private readonly confirmDialog = inject(ConfirmDialogService);

  protected readonly mostrarFormulario = signal(false);
  protected readonly usuarioEditando = signal<Usuario | null>(null);

  protected readonly usuariosResource = httpResource<Usuario[]>(() => ({
    url: `${environment.apiUrl}/usuarios`,
  }));

  protected abrirCreacion(): void {
    this.usuarioEditando.set(null);
    this.mostrarFormulario.set(true);
  }

  protected abrirEdicion(usuario: Usuario): void {
    this.usuarioEditando.set(usuario);
    this.mostrarFormulario.set(true);
  }

  protected cerrarFormulario(): void {
    this.mostrarFormulario.set(false);
  }

  protected onGuardado(): void {
    this.mostrarFormulario.set(false);
    this.usuariosResource.reload();
  }

  protected async confirmarDesactivacion(usuario: Usuario): Promise<void> {
    const confirmado = await this.confirmDialog.confirmar({
      titulo: 'Desactivar usuario',
      mensaje: `¿Desactivar a "${usuario.nombreUsuario}"? No podrá iniciar sesión, pero sus datos históricos se conservarán.`,
      textoConfirmar: 'Desactivar',
      peligroso: true,
    });

    if (!confirmado) return;

    this.usuarioService.eliminar(usuario.id).subscribe({
      next: () => this.usuariosResource.reload(),
    });
  }
}
