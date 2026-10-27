import { ChangeDetectionStrategy, Component, inject, input, output, signal } from '@angular/core';
import { Nota } from '../../../../core/models';
import { PersonajeService } from '../../../../core/services/personaje.service';
import { ConfirmDialogService } from '../../../../shared/components/confirm-dialog/confirm-dialog.service';

@Component({
  selector: 'app-notes-tab',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="notes-tab">
      @if (notas().length === 0) {
        <p class="empty-state">No hay notas todavía.</p>
      } @else {
        <div class="notes-tab__grid">
          @for (nota of notas(); track nota.id) {
            <div class="notes-tab__nota">
              <div class="notes-tab__nota-header">
                <input
                  class="input notes-tab__titulo"
                  type="text"
                  placeholder="Título"
                  [value]="nota.titulo ?? ''"
                  [disabled]="soloLectura()"
                  (blur)="guardarTitulo(nota, $any($event.target).value)"
                />
                @if (!soloLectura()) {
                  <button type="button" class="btn btn--danger btn--sm" (click)="eliminarNota(nota)">✕</button>
                }
              </div>
              <textarea
                class="textarea notes-tab__contenido"
                placeholder="Escribe aquí..."
                [disabled]="soloLectura()"
                (blur)="guardarContenido(nota, $any($event.target).value)"
              >{{ nota.contenido }}</textarea>
            </div>
          }
        </div>
      }

      @if (!soloLectura()) {
        <button type="button" class="btn btn--primary" [disabled]="creando()" (click)="crearNota()">
          {{ creando() ? 'Creando...' : '+ Nueva nota' }}
        </button>
      }
    </div>
  `,
  styleUrl: './notes-tab.component.scss',
})
export class NotesTabComponent {
  readonly personajeId = input.required<number>();
  readonly notas = input.required<Nota[]>();
  readonly soloLectura = input.required<boolean>();
  readonly cambio = output<void>();

  private readonly personajeService = inject(PersonajeService);
  private readonly confirmDialog = inject(ConfirmDialogService);

  protected readonly creando = signal(false);

  protected crearNota(): void {
    this.creando.set(true);
    this.personajeService.agregarNota(this.personajeId(), { titulo: 'Nueva nota', contenido: '' }).subscribe({
      next: () => {
        this.creando.set(false);
        this.cambio.emit();
      },
      error: () => this.creando.set(false),
    });
  }

  protected guardarTitulo(nota: Nota, nuevoTitulo: string): void {
    if (this.soloLectura() || nuevoTitulo === (nota.titulo ?? '')) return;

    this.personajeService.actualizarNota(this.personajeId(), nota.id, { titulo: nuevoTitulo, contenido: nota.contenido }).subscribe({
      next: () => this.cambio.emit(),
    });
  }

  protected guardarContenido(nota: Nota, nuevoContenido: string): void {
    if (this.soloLectura() || nuevoContenido === (nota.contenido ?? '')) return;

    this.personajeService.actualizarNota(this.personajeId(), nota.id, { titulo: nota.titulo, contenido: nuevoContenido }).subscribe({
      next: () => this.cambio.emit(),
    });
  }

  protected async eliminarNota(nota: Nota): Promise<void> {
    const confirmado = await this.confirmDialog.confirmar({
      titulo: 'Eliminar nota',
      mensaje: `¿Eliminar la nota "${nota.titulo || 'Sin título'}"? Esta acción no se puede deshacer.`,
      textoConfirmar: 'Eliminar',
      peligroso: true,
    });

    if (!confirmado) return;

    this.personajeService.eliminarNota(this.personajeId(), nota.id).subscribe({
      next: () => this.cambio.emit(),
    });
  }
}
