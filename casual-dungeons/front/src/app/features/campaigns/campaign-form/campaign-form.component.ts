import { ChangeDetectionStrategy, Component, ElementRef, computed, effect, inject, input, output, signal, viewChild } from '@angular/core';
import { CampaniaService } from '../../../core/services/campania.service';
import { UsuarioService } from '../../../core/services/usuario.service';
import { ArchivoService } from '../../../core/services/archivo.service';
import { UsuarioBasico } from '../../../core/models';

interface EstadoFormulario {
  nombre: string;
  descripcion: string;
  imagenUrl: string;
  fechaInicio: string;
  fechaFinalizacion: string;
  estado: 'Activa' | 'Finalizada';
  jugadoresSeleccionados: Set<number>;
}

function hoyComoISO(): string {
  return new Date().toISOString().slice(0, 10);
}

function estadoVacio(): EstadoFormulario {
  return {
    nombre: '',
    descripcion: '',
    imagenUrl: '',
    fechaInicio: hoyComoISO(),
    fechaFinalizacion: '',
    estado: 'Activa',
    jugadoresSeleccionados: new Set<number>(),
  };
}

/**
 * Modal de creación/edición de campaña. Si campaniaId() es null,
 * crea una campaña nueva; si tiene un valor, carga y edita esa
 * campaña existente.
 */
@Component({
  selector: 'app-campaign-form',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="dialog-overlay" (click)="cerrado.emit()">
      <div class="dialog campaign-form" (click)="$event.stopPropagation()">
        <h2 class="dialog__titulo">{{ campaniaId() ? 'Editar campaña' : 'Crear campaña' }}</h2>

        <div class="field">
          <label class="field__label" for="nombre">Nombre</label>
          <input id="nombre" class="input" type="text" [value]="estado().nombre" (input)="actualizarCampo('nombre', $any($event.target).value)" />
        </div>

        <div class="field">
          <label class="field__label" for="descripcion">Descripción</label>
          <textarea
            id="descripcion"
            class="textarea"
            [value]="estado().descripcion"
            (input)="actualizarCampo('descripcion', $any($event.target).value)"
          ></textarea>
        </div>

        <div class="field">
          <span class="field__label">Imagen</span>

          @if (vistaPreviaUrl()) {
            <img class="campaign-form__preview" [src]="vistaPreviaUrl()" alt="Vista previa de la imagen de campaña" />
          }

          <input
            #inputArchivo
            class="campaign-form__input-archivo"
            type="file"
            accept="image/jpeg,image/png,image/webp,image/gif"
            (change)="onArchivoSeleccionado($any($event.target).files)"
          />

          <div class="campaign-form__acciones-imagen">
            <button type="button" class="btn btn--ghost btn--sm" [disabled]="subiendoImagen()" (click)="inputArchivo.click()">
              {{ subiendoImagen() ? 'Subiendo...' : estado().imagenUrl ? 'Cambiar imagen' : 'Elegir imagen' }}
            </button>
            @if (estado().imagenUrl) {
              <button type="button" class="btn btn--ghost btn--sm" [disabled]="subiendoImagen()" (click)="quitarImagen()">Quitar</button>
            }
          </div>

          @if (errorImagen()) {
            <p class="field__error">{{ errorImagen() }}</p>
          }
        </div>

        <div class="campaign-form__fila">
          <div class="field">
            <label class="field__label" for="fechaInicio">Fecha de inicio</label>
            <input
              id="fechaInicio"
              class="input"
              type="date"
              [value]="estado().fechaInicio"
              (input)="actualizarCampo('fechaInicio', $any($event.target).value)"
            />
          </div>

          @if (campaniaId()) {
            <div class="field">
              <label class="field__label" for="fechaFinalizacion">Fecha de finalización</label>
              <input
                id="fechaFinalizacion"
                class="input"
                type="date"
                [value]="estado().fechaFinalizacion"
                (input)="actualizarCampo('fechaFinalizacion', $any($event.target).value)"
              />
            </div>
          }
        </div>

        @if (campaniaId()) {
          <div class="field">
            <label class="field__label" for="estado">Estado</label>
            <select id="estado" class="select" [value]="estado().estado" (change)="actualizarCampo('estado', $any($event.target).value)">
              <option value="Activa">Activa</option>
              <option value="Finalizada">Finalizada</option>
            </select>
          </div>
        }

        <div class="field">
          <span class="field__label">Jugadores</span>
          @if (jugadoresDisponibles().length === 0) {
            <p class="campaign-form__sin-jugadores">No hay jugadores registrados todavía.</p>
          } @else {
            <div class="campaign-form__jugadores">
              @for (jugador of jugadoresDisponibles(); track jugador.id) {
                <label class="campaign-form__jugador">
                  <input
                    type="checkbox"
                    [checked]="estado().jugadoresSeleccionados.has(jugador.id)"
                    (change)="alternarJugador(jugador.id)"
                  />
                  {{ jugador.nombreUsuario }}
                </label>
              }
            </div>
          }
        </div>

        @if (errorGuardado()) {
          <p class="field__error">{{ errorGuardado() }}</p>
        }

        <div class="dialog__acciones">
          <button type="button" class="btn btn--ghost" (click)="cerrado.emit()">Cancelar</button>
          <button type="button" class="btn btn--primary" [disabled]="guardando() || !nombreValido()" (click)="guardar()">
            {{ guardando() ? 'Guardando...' : 'Guardar' }}
          </button>
        </div>
      </div>
    </div>
  `,
  styleUrl: './campaign-form.component.scss',
})
export class CampaignFormComponent {
  readonly campaniaId = input<number | null>(null);
  readonly cerrado = output<void>();
  readonly guardado = output<void>();

  private readonly campaniaService = inject(CampaniaService);
  private readonly usuarioService = inject(UsuarioService);
  private readonly archivoService = inject(ArchivoService);

  private readonly inputArchivo = viewChild<ElementRef<HTMLInputElement>>('inputArchivo');

  protected readonly estado = signal<EstadoFormulario>(estadoVacio());
  protected readonly jugadoresDisponibles = signal<UsuarioBasico[]>([]);
  protected readonly guardando = signal(false);
  protected readonly errorGuardado = signal<string | null>(null);

  protected readonly subiendoImagen = signal(false);
  protected readonly errorImagen = signal<string | null>(null);

  // Vista previa resuelta contra el origen del backend, igual que en
  // campaign-card: la URL guardada en el estado es relativa
  // (/uploads/...) y no se vería si se usara directamente.
  protected readonly vistaPreviaUrl = computed(() => this.archivoService.urlAbsoluta(this.estado().imagenUrl || null));

  protected readonly nombreValido = computed(() => this.estado().nombre.trim().length > 0);

  constructor() {
    this.usuarioService.getJugadores().subscribe((jugadores) => this.jugadoresDisponibles.set(jugadores));

    effect(() => {
      const id = this.campaniaId();
      if (id !== null) {
        this.campaniaService.getDetalle(id).subscribe((detalle) => {
          this.estado.set({
            nombre: detalle.nombre,
            descripcion: detalle.descripcion ?? '',
            imagenUrl: detalle.imagenUrl ?? '',
            fechaInicio: detalle.fechaInicio.slice(0, 10),
            fechaFinalizacion: detalle.fechaFinalizacion?.slice(0, 10) ?? '',
            estado: detalle.estado,
            jugadoresSeleccionados: new Set(detalle.participantes.map((p) => p.usuarioId)),
          });
        });
      } else {
        this.estado.set(estadoVacio());
      }
    });
  }

  protected actualizarCampo<K extends keyof EstadoFormulario>(campo: K, valor: EstadoFormulario[K]): void {
    this.estado.update((actual) => ({ ...actual, [campo]: valor }));
  }

  protected alternarJugador(jugadorId: number): void {
    this.estado.update((actual) => {
      const nuevoSet = new Set(actual.jugadoresSeleccionados);
      if (nuevoSet.has(jugadorId)) {
        nuevoSet.delete(jugadorId);
      } else {
        nuevoSet.add(jugadorId);
      }
      return { ...actual, jugadoresSeleccionados: nuevoSet };
    });
  }

  /**
   * Sube la imagen apenas se selecciona el archivo (no al guardar el
   * formulario completo), y guarda la URL relativa devuelta por el
   * backend en el campo imagenUrl del estado.
   */
  protected onArchivoSeleccionado(archivos: FileList | null): void {
    const archivo = archivos?.[0];
    if (!archivo) return;

    this.errorImagen.set(null);
    this.subiendoImagen.set(true);

    this.archivoService.subirImagen(archivo).subscribe({
      next: (resultado) => {
        this.subiendoImagen.set(false);
        this.actualizarCampo('imagenUrl', resultado.url);
      },
      error: () => {
        this.subiendoImagen.set(false);
        this.errorImagen.set('No se pudo subir la imagen. Verifica que sea jpg, png, webp o gif y pese menos de 5 MB.');
      },
    });

    // Permite volver a elegir el mismo archivo más adelante (ej. tras
    // quitarlo) sin que el navegador ignore el cambio por ser el
    // mismo valor de input.
    const input = this.inputArchivo()?.nativeElement;
    if (input) input.value = '';
  }

  protected quitarImagen(): void {
    this.actualizarCampo('imagenUrl', '');
    this.errorImagen.set(null);
  }

  protected guardar(): void {
    if (!this.nombreValido()) return;

    this.guardando.set(true);
    this.errorGuardado.set(null);

    const valores = this.estado();
    const jugadoresIds = Array.from(valores.jugadoresSeleccionados);

    const id = this.campaniaId();

    const observable = id
      ? this.campaniaService.editar(id, {
          nombre: valores.nombre,
          descripcion: valores.descripcion || null,
          imagenUrl: valores.imagenUrl || null,
          fechaInicio: valores.fechaInicio,
          fechaFinalizacion: valores.fechaFinalizacion || null,
          estado: valores.estado,
          jugadoresIds,
        })
      : this.campaniaService.crear({
          nombre: valores.nombre,
          descripcion: valores.descripcion || null,
          imagenUrl: valores.imagenUrl || null,
          fechaInicio: valores.fechaInicio,
          jugadoresIds,
        });

    observable.subscribe({
      next: () => {
        this.guardando.set(false);
        this.guardado.emit();
      },
      error: () => {
        this.guardando.set(false);
        this.errorGuardado.set('No se pudo guardar la campaña. Verifica los datos e intenta de nuevo.');
      },
    });
  }
}
