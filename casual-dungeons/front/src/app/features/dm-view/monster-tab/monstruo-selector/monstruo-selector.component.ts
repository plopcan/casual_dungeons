import { ChangeDetectionStrategy, Component, inject, input, output, signal } from '@angular/core';
import { MonstruoCatalogoService } from '../../../../core/services/monstruo-catalogo.service';
import { MonstruoCampaniaService } from '../../../../core/services/monstruo-campania.service';
import { MonstruoCatalogoFicha, MonstruoCatalogoResumen } from '../../../../core/models';

/**
 * Modal "Añadir desde catálogo": lista de monstruos precreados
 * (SRD 5.1) con buscador, vista previa del stat block completo, y
 * confirmación para añadirlo como MonstruoCampania (copia TODOS los
 * campos del catálogo, a diferencia del antiguo selector de NPC que
 * los aplanaba como texto en las Notas).
 */
@Component({
  selector: 'app-monstruo-selector',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="dialog-overlay" (click)="cerrado.emit()">
      <div class="dialog monstruo-selector" (click)="$event.stopPropagation()">
        <h2 class="dialog__titulo">Añadir monstruo desde catálogo</h2>

        @if (!monstruoSeleccionado()) {
          <div class="field">
            <input
              class="input"
              type="text"
              placeholder="Buscar por nombre..."
              [value]="termino()"
              (input)="onBuscar($any($event.target).value)"
            />
          </div>

          @if (cargando()) {
            <p class="empty-state">Buscando...</p>
          } @else if (errorCarga()) {
            <p class="field__error">{{ errorCarga() }}</p>
          } @else if (resultados().length === 0) {
            <p class="empty-state">No se encontraron monstruos con ese nombre.</p>
          } @else {
            <div class="monstruo-selector__lista">
              @for (monstruo of resultados(); track monstruo.id) {
                <button type="button" class="monstruo-selector__fila" (click)="seleccionar(monstruo)">
                  <span class="monstruo-selector__nombre">{{ monstruo.nombre }}</span>
                  <span class="monstruo-selector__meta">{{ monstruo.tamano }} {{ monstruo.tipo }} · Desafío {{ monstruo.desafio }}</span>
                  <span class="monstruo-selector__stats">PV {{ monstruo.puntosVidaPromedio }} · CA {{ monstruo.claseArmadura }}</span>
                </button>
              }
            </div>
          }

          <div class="dialog__acciones">
            <button type="button" class="btn btn--ghost" (click)="cerrado.emit()">Cancelar</button>
          </div>
        } @else {
          <div class="monstruo-selector__ficha">
            <button type="button" class="btn btn--ghost btn--sm" (click)="volverALista()">&larr; Volver a la lista</button>

            <h3>{{ monstruoSeleccionado()!.nombre }}</h3>
            <p class="monstruo-selector__meta">
              {{ monstruoSeleccionado()!.tamano }} {{ monstruoSeleccionado()!.tipo }}
              @if (monstruoSeleccionado()!.alineamiento) {
                , {{ monstruoSeleccionado()!.alineamiento }}
              }
            </p>
            <p class="monstruo-selector__meta">
              CA {{ monstruoSeleccionado()!.claseArmadura }} ({{ monstruoSeleccionado()!.descripcionArmadura || 'sin armadura' }}) · PV
              {{ monstruoSeleccionado()!.puntosVidaPromedio }} ({{ monstruoSeleccionado()!.dadoGolpe }}) · Velocidad
              {{ monstruoSeleccionado()!.velocidad }}
            </p>

            <div class="monstruo-selector__atributos">
              <span>FUE {{ monstruoSeleccionado()!.fuerza }}</span>
              <span>DES {{ monstruoSeleccionado()!.destreza }}</span>
              <span>CON {{ monstruoSeleccionado()!.constitucion }}</span>
              <span>INT {{ monstruoSeleccionado()!.inteligencia }}</span>
              <span>SAB {{ monstruoSeleccionado()!.sabiduria }}</span>
              <span>CAR {{ monstruoSeleccionado()!.carisma }}</span>
            </div>

            @if (monstruoSeleccionado()!.salvaciones) {
              <p class="monstruo-selector__linea"><strong>Salvaciones.</strong> {{ monstruoSeleccionado()!.salvaciones }}</p>
            }
            @if (monstruoSeleccionado()!.habilidades) {
              <p class="monstruo-selector__linea"><strong>Habilidades.</strong> {{ monstruoSeleccionado()!.habilidades }}</p>
            }
            @if (monstruoSeleccionado()!.resistenciasDano) {
              <p class="monstruo-selector__linea"><strong>Resistencias.</strong> {{ monstruoSeleccionado()!.resistenciasDano }}</p>
            }
            @if (monstruoSeleccionado()!.inmunidadesDano) {
              <p class="monstruo-selector__linea"><strong>Inmunidades al daño.</strong> {{ monstruoSeleccionado()!.inmunidadesDano }}</p>
            }
            @if (monstruoSeleccionado()!.inmunidadesEstado) {
              <p class="monstruo-selector__linea"><strong>Inmunidades a estados.</strong> {{ monstruoSeleccionado()!.inmunidadesEstado }}</p>
            }
            @if (monstruoSeleccionado()!.sentidos) {
              <p class="monstruo-selector__linea"><strong>Sentidos.</strong> {{ monstruoSeleccionado()!.sentidos }}</p>
            }
            @if (monstruoSeleccionado()!.idiomas) {
              <p class="monstruo-selector__linea"><strong>Idiomas.</strong> {{ monstruoSeleccionado()!.idiomas }}</p>
            }
            <p class="monstruo-selector__linea">
              <strong>Desafío.</strong> {{ monstruoSeleccionado()!.desafio }} ({{ monstruoSeleccionado()!.puntosExperiencia }} PX)
            </p>

            @for (detalle of monstruoSeleccionado()!.detalles; track detalle.nombre) {
              <p class="monstruo-selector__linea"><strong>{{ detalle.nombre }}.</strong> {{ detalle.descripcion }}</p>
            }

            <p class="monstruo-selector__fuente">Fuente: {{ monstruoSeleccionado()!.fuente }}</p>

            <div class="field">
              <label class="field__label" for="nombrePersonalizado">Nombre para esta campaña (opcional)</label>
              <input
                id="nombrePersonalizado"
                class="input"
                type="text"
                [placeholder]="monstruoSeleccionado()!.nombre"
                [value]="nombrePersonalizado()"
                (input)="nombrePersonalizado.set($any($event.target).value)"
              />
            </div>

            @if (errorGuardado()) {
              <p class="field__error">{{ errorGuardado() }}</p>
            }

            <div class="dialog__acciones">
              <button type="button" class="btn btn--ghost" (click)="cerrado.emit()">Cancelar</button>
              <button type="button" class="btn btn--primary" [disabled]="anadiendo()" (click)="confirmarAlta()">
                {{ anadiendo() ? 'Añadiendo...' : 'Añadir a la campaña' }}
              </button>
            </div>
          </div>
        }
      </div>
    </div>
  `,
  styleUrl: './monstruo-selector.component.scss',
})
export class MonstruoSelectorComponent {
  readonly campaniaId = input.required<number>();
  readonly cerrado = output<void>();
  readonly creado = output<void>();

  private readonly monstruoCatalogoService = inject(MonstruoCatalogoService);
  private readonly monstruoCampaniaService = inject(MonstruoCampaniaService);

  protected readonly termino = signal('');
  protected readonly cargando = signal(false);
  protected readonly errorCarga = signal<string | null>(null);
  protected readonly resultados = signal<MonstruoCatalogoResumen[]>([]);

  protected readonly monstruoSeleccionado = signal<MonstruoCatalogoFicha | null>(null);
  protected readonly nombrePersonalizado = signal('');
  protected readonly anadiendo = signal(false);
  protected readonly errorGuardado = signal<string | null>(null);

  private debounceTimer: ReturnType<typeof setTimeout> | null = null;

  constructor() {
    this.buscar('');
  }

  protected onBuscar(valor: string): void {
    this.termino.set(valor);

    // Debounce simple para no lanzar una petición por cada tecla.
    if (this.debounceTimer) clearTimeout(this.debounceTimer);
    this.debounceTimer = setTimeout(() => this.buscar(valor), 300);
  }

  private buscar(termino: string): void {
    this.cargando.set(true);
    this.errorCarga.set(null);
    this.monstruoCatalogoService.getTodos(termino || undefined).subscribe({
      next: (monstruos) => {
        this.cargando.set(false);
        this.resultados.set(monstruos);
      },
      error: (error: unknown) => {
        this.cargando.set(false);
        this.resultados.set([]);

        const status = (error as { status?: number })?.status;
        if (status === 403) {
          this.errorCarga.set('Solo el Dungeon Master de la campaña puede usar el catálogo de monstruos.');
        } else if (status === 401) {
          this.errorCarga.set('Tu sesión ha expirado. Vuelve a iniciar sesión.');
        } else if (status === 0) {
          this.errorCarga.set('No se pudo conectar con el servidor. Comprueba que el backend esté en marcha.');
        } else {
          this.errorCarga.set('No se pudo cargar el catálogo de monstruos. Inténtalo de nuevo.');
        }
      },
    });
  }

  protected seleccionar(monstruo: MonstruoCatalogoResumen): void {
    this.monstruoCatalogoService.getFicha(monstruo.id).subscribe((ficha) => {
      this.monstruoSeleccionado.set(ficha);
      this.nombrePersonalizado.set('');
    });
  }

  protected volverALista(): void {
    this.monstruoSeleccionado.set(null);
    this.errorGuardado.set(null);
  }

  protected confirmarAlta(): void {
    const monstruo = this.monstruoSeleccionado();
    if (!monstruo) return;

    this.anadiendo.set(true);
    this.errorGuardado.set(null);

    this.monstruoCampaniaService
      .crearDesdeCatalogo(this.campaniaId(), {
        monstruoCatalogoId: monstruo.id,
        nombrePersonalizado: this.nombrePersonalizado().trim() || null,
      })
      .subscribe({
        next: () => {
          this.anadiendo.set(false);
          this.creado.emit();
        },
        error: () => {
          this.anadiendo.set(false);
          this.errorGuardado.set('No se pudo añadir el monstruo. Inténtalo de nuevo.');
        },
      });
  }
}
