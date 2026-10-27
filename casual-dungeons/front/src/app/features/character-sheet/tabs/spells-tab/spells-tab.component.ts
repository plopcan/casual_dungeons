import { ChangeDetectionStrategy, Component, computed, inject, input, output, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { HechizoCatalogo, PersonajeEspacioConjuro, PersonajeHechizo } from '../../../../core/models';
import { HechizoService } from '../../../../core/services/hechizo.service';
import { PersonajeService } from '../../../../core/services/personaje.service';
import { ConfirmDialogService } from '../../../../shared/components/confirm-dialog/confirm-dialog.service';

@Component({
  selector: 'app-spells-tab',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="spells-tab">
      <div class="spells-tab__espacios-header">
        <h4>Espacios de conjuro</h4>
        @if (!soloLectura()) {
          <div class="spells-tab__espacios-botones">
            <button type="button" class="btn btn--secondary" (click)="resetear()">
              Reiniciar espacios usados
            </button>
            <button type="button" class="btn btn--primary" (click)="abrirEdicion()">Editar</button>
          </div>
        }
      </div>

      @if (nivelesAMostrar().length === 0) {
        <p class="empty-state">
          {{ soloLectura() ? 'Este personaje no tiene espacios de conjuro configurados.' : 'Todavía no has configurado espacios de conjuro. Pulsa "Editar" para indicar cuántos tienes por nivel.' }}
        </p>
      } @else {
        <div class="spells-tab__espacios">
          @for (nivel of nivelesAMostrar(); track nivel) {
            <div class="spells-tab__espacio-fila">
              <span class="spells-tab__espacio-nivel">Nivel {{ nivel }}</span>

              <div class="spells-tab__espacio-pips">
                @for (i of maxCheckboxes; track i) {
                  @if (i < totalPorNivel(nivel)) {
                    <button
                      type="button"
                      class="spells-tab__pip"
                      [class.spells-tab__pip--disponible]="i >= usadosPorNivel(nivel)"
                      [disabled]="soloLectura()"
                      [title]="i < usadosPorNivel(nivel) ? 'Espacio gastado (clic para recuperar)' : 'Espacio disponible (clic para gastar)'"
                      (click)="togglePip(nivel, i)"
                    ></button>
                  }
                }
              </div>
            </div>
          }
        </div>
      }

      @if (editando()) {
        <div class="dialog-overlay" (click)="cerrarEdicion()">
          <div class="dialog" (click)="$event.stopPropagation()">
            <h2 class="dialog__titulo">Editar espacios de conjuro</h2>
            <p class="spells-tab__espacios-ayuda">
              Indica tú mismo cuántos espacios totales tienes en cada nivel según tu clase (o tu clase
              homebrew); no se calculan automáticamente. Máximo 13 por nivel.
            </p>

            <div class="spells-tab__edicion-grid">
              @for (nivel of todosLosNiveles; track nivel) {
                <div class="field">
                  <label class="field__label" [for]="'espacios-nivel-' + nivel">Nivel {{ nivel }}</label>
                  <input
                    [id]="'espacios-nivel-' + nivel"
                    class="input input--sm"
                    type="number"
                    min="0"
                    max="13"
                    [value]="totalesEnEdicion()[nivel] ?? 0"
                    (change)="actualizarTotalEdicion(nivel, $any($event.target).value)"
                  />
                </div>
              }
            </div>

            <div class="dialog__acciones">
              <button type="button" class="btn btn--ghost" (click)="cerrarEdicion()">Cancelar</button>
              <button type="button" class="btn btn--primary" [disabled]="guardandoEdicion()" (click)="guardarEdicion()">
                {{ guardandoEdicion() ? 'Guardando...' : 'Aceptar' }}
              </button>
            </div>
          </div>
        </div>
      }

      @if (hechizos().length === 0) {
        <p class="empty-state">Todavía no tienes hechizos conocidos.</p>
      } @else {
        <table class="spells-tab__tabla">
          <thead>
            <tr>
              <th>Hechizo</th>
              <th>Nivel</th>
              <th>Escuela</th>
              <th>Descripcion</th>
              <th>Preparado</th>
              @if (!soloLectura()) {
                <th></th>
              }
            </tr>
          </thead>
          <tbody>
            @for (hechizo of hechizos(); track hechizo.id) {
              <tr>
                <td>{{ hechizo.nombreHechizo }}</td>
                <td class="stat-figure">{{ hechizo.nivelHechizo === 0 ? 'Truco' : hechizo.nivelHechizo }}</td>
                <td>{{ hechizo.escuela || '—' }}</td>
                <td>{{ hechizo.descripcionHechizo || '—' }}</td>
                <td>
                  <input
                    type="checkbox"
                    [checked]="hechizo.preparado"
                    [disabled]="soloLectura()"
                    (change)="togglePreparado(hechizo)"
                  />
                </td>
                @if (!soloLectura()) {
                  <td>
                    <button type="button" class="btn btn--danger btn--sm" (click)="quitarHechizo(hechizo)">Quitar</button>
                  </td>
                }
              </tr>
            }
          </tbody>
        </table>
      }

      @if (!soloLectura()) {
        <div class="spells-tab__añadir">
          <select class="select" [value]="hechizoSeleccionadoId()" (change)="hechizoSeleccionadoId.set($any($event.target).value)">
            <option value="">Añadir hechizo del catálogo...</option>
            @for (hechizo of catalogoDisponible(); track hechizo.id) {
              <option [value]="hechizo.id">
                {{ hechizo.nombre }} ({{ hechizo.nivel === 0 ? 'Truco' : 'Nivel ' + hechizo.nivel }})
              </option>
            }
          </select>
          <button type="button" class="btn btn--primary btn--sm" [disabled]="!hechizoSeleccionadoId()" (click)="addHechizo()">
            + Añadir
          </button>
        </div>
      }
    </div>
  `,
  styleUrl: './spells-tab.component.scss',
})
export class SpellsTabComponent {
  readonly personajeId = input.required<number>();
  readonly hechizos = input.required<PersonajeHechizo[]>();
  readonly espaciosConjuro = input.required<PersonajeEspacioConjuro[]>();
  readonly soloLectura = input.required<boolean>();
  readonly cambio = output<void>();

  private readonly personajeService = inject(PersonajeService);
  private readonly hechizoService = inject(HechizoService);
  private readonly confirmDialog = inject(ConfirmDialogService);

  protected readonly catalogo = signal<HechizoCatalogo[]>([]);
  protected readonly hechizoSeleccionadoId = signal<string>('');

  /** D&D 5e: los conjuros van de nivel 1 a 9 (el nivel 0 son trucos, que no gastan espacio). */
  protected readonly todosLosNiveles = [1, 2, 3, 4, 5, 6, 7, 8, 9];

  /** Rejilla fija de 13 casillas por nivel; solo se muestran (des-ocultan) las primeras N según el total configurado. */
  protected readonly maxCheckboxes = Array.from({ length: 13 }, (_, i) => i);

  protected readonly catalogoDisponible = computed(() => {
    const idsConocidos = new Set(this.hechizos().map((h) => h.hechizoId));
    return this.catalogo().filter((h) => !idsConocidos.has(h.id));
  });

  /** Solo se muestran los niveles que ya tienen al menos 1 espacio total configurado. */
  protected readonly nivelesAMostrar = computed(() =>
    this.espaciosConjuro()
      .filter((e) => e.espaciosTotales > 0)
      .map((e) => e.nivelConjuro)
      .sort((a, b) => a - b),
  );

  // ---- Modal de edición de espacios ----
  protected readonly editando = signal(false);
  protected readonly guardandoEdicion = signal(false);
  protected readonly totalesEnEdicion = signal<Record<number, number>>({});

  constructor() {
    this.hechizoService.getAll().subscribe((catalogo) => this.catalogo.set(catalogo));
  }

  protected totalPorNivel(nivel: number): number {
    return this.espaciosConjuro().find((e) => e.nivelConjuro === nivel)?.espaciosTotales ?? 0;
  }

  protected usadosPorNivel(nivel: number): number {
    return this.espaciosConjuro().find((e) => e.nivelConjuro === nivel)?.espaciosUsados ?? 0;
  }

  protected abrirEdicion(): void {
    if (this.soloLectura()) return;

    const totales: Record<number, number> = {};
    for (const nivel of this.todosLosNiveles) {
      totales[nivel] = this.totalPorNivel(nivel);
    }
    this.totalesEnEdicion.set(totales);
    this.editando.set(true);
  }

  protected cerrarEdicion(): void {
    this.editando.set(false);
  }

  protected actualizarTotalEdicion(nivel: number, valor: string): void {
    const espaciosTotales = Math.max(0, Math.min(13, Number(valor) || 0));
    this.totalesEnEdicion.update((actual) => ({ ...actual, [nivel]: espaciosTotales }));
  }

  /**
   * Al aceptar, guarda solo los niveles cuyo total haya cambiado
   * respecto al valor actual, y des-oculta (o vuelve a ocultar) los
   * checkboxes correspondientes en cuanto se recarga la ficha.
   */
  protected guardarEdicion(): void {
    const cambios = this.todosLosNiveles
      .map((nivel) => ({ nivel, total: this.totalesEnEdicion()[nivel] ?? 0 }))
      .filter((cambio) => cambio.total !== this.totalPorNivel(cambio.nivel));

    if (cambios.length === 0) {
      this.editando.set(false);
      return;
    }

    this.guardandoEdicion.set(true);
    const peticiones = cambios.map(({ nivel, total }) =>
      this.personajeService.configurarEspacioConjuro(this.personajeId(), nivel, total),
    );

    forkJoin(peticiones).subscribe({
      next: () => {
        this.guardandoEdicion.set(false);
        this.editando.set(false);
        this.cambio.emit();
      },
      error: () => {
        this.guardandoEdicion.set(false);
      },
    });
  }

  /**
   * Cada espacio se representa como un checkbox circular. Un espacio
   * disponible (relleno) se puede pulsar para gastarlo; uno ya
   * gastado (vacío) se puede volver a pulsar para recuperarlo, sin
   * tener que esperar a reiniciar todos.
   */
  protected togglePip(nivel: number, indice: number): void {
    if (this.soloLectura()) return;

    const usados = this.usadosPorNivel(nivel);
    const accion$ =
      indice < usados
        ? this.personajeService.recuperarEspacioConjuro(this.personajeId(), nivel)
        : this.personajeService.gastarEspacioConjuro(this.personajeId(), nivel);

    accion$.subscribe({ next: () => this.cambio.emit() });
  }

  protected resetear(): void {
    if (this.soloLectura()) return;

    this.personajeService.resetearEspaciosConjuro(this.personajeId()).subscribe({
      next: () => this.cambio.emit(),
    });
  }

  protected togglePreparado(hechizo: PersonajeHechizo): void {
    if (this.soloLectura()) return;

    this.personajeService.togglePreparado(this.personajeId(), hechizo.id).subscribe({
      next: () => this.cambio.emit(),
    });
  }

  protected addHechizo(): void {
    const hechizoId = Number(this.hechizoSeleccionadoId());
    if (!hechizoId) return;

    this.personajeService.agregarHechizo(this.personajeId(), hechizoId).subscribe({
      next: () => {
        this.hechizoSeleccionadoId.set('');
        this.cambio.emit();
      },
    });
  }

  protected async quitarHechizo(hechizo: PersonajeHechizo): Promise<void> {
    const confirmado = await this.confirmDialog.confirmar({
      titulo: 'Quitar hechizo',
      mensaje: `¿Quitar "${hechizo.nombreHechizo}" de tus hechizos conocidos?`,
      textoConfirmar: 'Quitar',
      peligroso: true,
    });

    if (!confirmado) return;

    this.personajeService.quitarHechizo(this.personajeId(), hechizo.id).subscribe({
      next: () => this.cambio.emit(),
    });
  }
}
