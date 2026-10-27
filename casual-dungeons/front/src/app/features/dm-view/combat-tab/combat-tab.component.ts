import { httpResource } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, effect, inject, input, signal } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { Combate, MonstruoCampania, NPC, ParticipanteCombate, PersonajeResumen } from '../../../core/models';
import { CombateService } from '../../../core/services/combate.service';
import { NPCService } from '../../../core/services/npc.service';
import { MonstruoCampaniaService } from '../../../core/services/monstruo-campania.service';
import { RealtimeService } from '../../../core/services/realtime.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog.service';

/**
 * Pestaña de gestión de combate del DM: crea encuentros, añade
 * jugadores/NPCs a la iniciativa, permite reordenar mediante
 * drag & drop nativo (HTML5 Drag and Drop API, sin dependencias
 * externas) y persiste el nuevo orden en el backend.
 */
@Component({
  selector: 'app-combat-tab',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="combat-tab__header">
      <h2>Combate</h2>
      @if (!combateActivo()) {
        <button type="button" class="btn btn--primary" (click)="crearCombate()" [disabled]="creando()">
          {{ creando() ? 'Creando...' : '+ Iniciar combate' }}
        </button>
      }
    </div>

    @if (!combateActivo()) {
      <p class="empty-state">No hay un combate en curso. Inicia uno para gestionar la iniciativa.</p>
    } @else {
      <div class="combat-tab__panel">
        <div class="combat-tab__añadir">
          <select class="select" [value]="seleccionPersonajeId()" (change)="seleccionPersonajeId.set($any($event.target).value)">
            <option value="">Añadir jugador...</option>
            @for (personaje of personajesDisponibles(); track personaje.id) {
              <option [value]="personaje.id">{{ personaje.nombrePersonaje }}</option>
            }
          </select>
          <button type="button" class="btn btn--ghost btn--sm" (click)="addPersonaje()">Añadir</button>

          <select class="select" [value]="seleccionNpcId()" (change)="seleccionNpcId.set($any($event.target).value)">
            <option value="">Añadir NPC...</option>
            @for (npc of npcsDisponibles(); track npc.id) {
              <option [value]="npc.id">{{ npc.nombre }}</option>
            }
          </select>
          <button type="button" class="btn btn--ghost btn--sm" (click)="addNpc()">Añadir</button>

          <select class="select" [value]="seleccionMonstruoId()" (change)="seleccionMonstruoId.set($any($event.target).value)">
            <option value="">Añadir monstruo...</option>
            @for (monstruo of monstruosDisponibles(); track monstruo.id) {
              <option [value]="monstruo.id">{{ monstruo.nombre }}</option>
            }
          </select>
          <button type="button" class="btn btn--ghost btn--sm" (click)="addMonstruo()">Añadir</button>
        </div>

        <ol class="combat-tab__lista">
          @for (participante of combateActivo()!.participantes; track participante.id) {
            <li
              class="combat-tab__item"
              [class.combat-tab__item--arrastrando]="idArrastrando() === participante.id"
              draggable="true"
              (dragstart)="onDragStart(participante.id)"
              (dragover)="onDragOver($event)"
              (drop)="onDrop(participante.id)"
              (dragend)="idArrastrando.set(null)"
            >
              <span class="combat-tab__handle" aria-hidden="true">⠿</span>
              <span class="combat-tab__orden">{{ $index + 1 }}</span>
              <span class="combat-tab__nombre">{{ participante.nombre }}</span>
              <span class="badge badge--muted">{{ etiquetaOrigen(participante) }}</span>
              @if (participante.puntosVidaActuales !== null) {
                <span class="stat-figure combat-tab__pv">PV {{ participante.puntosVidaActuales }}</span>
              }
              <button
                type="button"
                class="btn btn--danger btn--sm"
                (click)="eliminarParticipante(participante)"
                aria-label="Quitar del combate"
              >
                ✕
              </button>
            </li>
          }
        </ol>

        <div class="combat-tab__acciones-finales">
          <button type="button" class="btn btn--ghost" (click)="finalizarCombate()">Finalizar combate</button>
        </div>
      </div>
    }
  `,
  styleUrl: './combat-tab.component.scss',
})
export class CombatTabComponent {
  readonly campaniaId = input.required<number>();

  private readonly combateService = inject(CombateService);
  private readonly npcService = inject(NPCService);
  private readonly monstruoCampaniaService = inject(MonstruoCampaniaService);
  private readonly realtimeService = inject(RealtimeService);
  private readonly confirmDialog = inject(ConfirmDialogService);

  protected readonly creando = signal(false);
  protected readonly idArrastrando = signal<number | null>(null);
  protected readonly seleccionPersonajeId = signal<string>('');
  protected readonly seleccionNpcId = signal<string>('');
  protected readonly seleccionMonstruoId = signal<string>('');

  // Combate activo (EnPreparacion o EnCurso) de la campaña, si existe.
  protected readonly combatesResource = httpResource<Combate[]>(() => ({
    url: `${environment.apiUrl}/campanias/${this.campaniaId()}/combates`,
  }));

  protected readonly combateActivo = computed(() => {
    const combates = this.combatesResource.value() ?? [];
    return combates.find((c) => c.estado !== 'Finalizado') ?? null;
  });

  protected readonly personajesResource = httpResource<PersonajeResumen[]>(() => ({
    url: `${environment.apiUrl}/campanias/${this.campaniaId()}/personajes`,
  }));

  protected readonly npcsResource = httpResource<NPC[]>(() => ({
    url: `${environment.apiUrl}/campanias/${this.campaniaId()}/npcs`,
  }));

  protected readonly monstruosResource = httpResource<MonstruoCampania[]>(() => ({
    url: `${environment.apiUrl}/campanias/${this.campaniaId()}/monstruos-campania`,
  }));

  protected readonly personajesDisponibles = computed(() => {
    const combate = this.combateActivo();
    const todos = this.personajesResource.value() ?? [];
    if (!combate) return todos;
    const idsEnCombate = new Set(combate.participantes.map((p) => p.personajeId).filter((id) => id !== null));
    return todos.filter((p) => !idsEnCombate.has(p.id));
  });

  protected readonly npcsDisponibles = computed(() => {
    const combate = this.combateActivo();
    const todos = this.npcsResource.value() ?? [];
    if (!combate) return todos;
    const idsEnCombate = new Set(combate.participantes.map((p) => p.npcId).filter((id) => id !== null));
    return todos.filter((n) => !idsEnCombate.has(n.id));
  });

  protected readonly monstruosDisponibles = computed(() => {
    const combate = this.combateActivo();
    const todos = this.monstruosResource.value() ?? [];
    if (!combate) return todos;
    const idsEnCombate = new Set(combate.participantes.map((p) => p.monstruoCampaniaId).filter((id) => id !== null));
    return todos.filter((m) => !idsEnCombate.has(m.id));
  });

  constructor() {
    effect(() => {
      if (this.realtimeService.combateActualizado() !== null) {
        this.combatesResource.reload();
      }
    });
  }

  protected crearCombate(): void {
    this.creando.set(true);
    this.combateService.crear({ campaniaId: this.campaniaId() }).subscribe({
      next: () => {
        this.creando.set(false);
        this.combatesResource.reload();
      },
      error: () => this.creando.set(false),
    });
  }

  protected addPersonaje(): void {
    const combate = this.combateActivo();
    const id = Number(this.seleccionPersonajeId());
    if (!combate || !id) return;

    this.combateService
      .agregarParticipante(combate.id, { personajeId: id, npcId: null, monstruoCampaniaId: null, valorIniciativa: null })
      .subscribe({
        next: () => {
          this.seleccionPersonajeId.set('');
          this.combatesResource.reload();
        },
      });
  }

  protected addNpc(): void {
    const combate = this.combateActivo();
    const id = Number(this.seleccionNpcId());
    if (!combate || !id) return;

    this.combateService
      .agregarParticipante(combate.id, { personajeId: null, npcId: id, monstruoCampaniaId: null, valorIniciativa: null })
      .subscribe({
        next: () => {
          this.seleccionNpcId.set('');
          this.combatesResource.reload();
        },
      });
  }

  protected addMonstruo(): void {
    const combate = this.combateActivo();
    const id = Number(this.seleccionMonstruoId());
    if (!combate || !id) return;

    this.combateService
      .agregarParticipante(combate.id, { personajeId: null, npcId: null, monstruoCampaniaId: id, valorIniciativa: null })
      .subscribe({
        next: () => {
          this.seleccionMonstruoId.set('');
          this.combatesResource.reload();
        },
      });
  }

  protected etiquetaOrigen(participante: ParticipanteCombate): string {
    switch (participante.origen) {
      case 'Personaje':
        return 'Jugador';
      case 'NPC':
        return 'NPC';
      default:
        return 'Monstruo';
    }
  }

  protected async eliminarParticipante(participante: ParticipanteCombate): Promise<void> {
    const combate = this.combateActivo();
    if (!combate) return;

    const confirmado = await this.confirmDialog.confirmar({
      titulo: 'Quitar del combate',
      mensaje: `¿Quitar a "${participante.nombre}" del orden de iniciativa?`,
      textoConfirmar: 'Quitar',
      peligroso: true,
    });

    if (!confirmado) return;

    this.combateService.eliminarParticipante(combate.id, participante.id).subscribe({
      next: () => this.combatesResource.reload(),
    });
  }

  protected async finalizarCombate(): Promise<void> {
    const combate = this.combateActivo();
    if (!combate) return;

    const confirmado = await this.confirmDialog.confirmar({
      titulo: 'Finalizar combate',
      mensaje: '¿Finalizar este combate? Podrás iniciar uno nuevo después.',
      textoConfirmar: 'Finalizar',
    });

    if (!confirmado) return;

    this.combateService.finalizar(combate.id).subscribe({
      next: () => this.combatesResource.reload(),
    });
  }

  // ---------------- Drag & Drop nativo ----------------

  protected onDragStart(participanteId: number): void {
    this.idArrastrando.set(participanteId);
  }

  protected onDragOver(event: DragEvent): void {
    // Necesario para permitir el drop en el navegador.
    event.preventDefault();
  }

  protected onDrop(participanteIdDestino: number): void {
    const combate = this.combateActivo();
    const idOrigen = this.idArrastrando();
    if (!combate || idOrigen === null || idOrigen === participanteIdDestino) {
      this.idArrastrando.set(null);
      return;
    }

    const ordenActual = combate.participantes.map((p) => p.id);
    const indiceOrigen = ordenActual.indexOf(idOrigen);
    const indiceDestino = ordenActual.indexOf(participanteIdDestino);

    if (indiceOrigen === -1 || indiceDestino === -1) {
      this.idArrastrando.set(null);
      return;
    }

    const nuevoOrden = [...ordenActual];
    nuevoOrden.splice(indiceOrigen, 1);
    nuevoOrden.splice(indiceDestino, 0, idOrigen);

    this.idArrastrando.set(null);

    this.combateService.reordenar(combate.id, { participanteCombateIdsEnOrden: nuevoOrden }).subscribe({
      next: () => this.combatesResource.reload(),
    });
  }
}
