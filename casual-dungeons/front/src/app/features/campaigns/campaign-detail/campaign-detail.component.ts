import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, inject, input, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { PersonajeService } from '../../../core/services/personaje.service';
import { RealtimeService } from '../../../core/services/realtime.service';
import { PersonajeResumen } from '../../../core/models';
import { PlayersTabComponent } from '../../dm-view/players-tab/players-tab.component';
import { NpcTabComponent } from '../../dm-view/npc-tab/npc-tab.component';
import { MonsterTabComponent } from '../../dm-view/monster-tab/monster-tab.component';
import { CombatTabComponent } from '../../dm-view/combat-tab/combat-tab.component';
import { PersonajeFormComponent } from '../personaje-form/personaje-form.component';

type PestaniaDM = 'jugadores' | 'npcs' | 'monstruos' | 'combate';

/**
 * Punto de entrada a una campaña. Si el usuario es el Dungeon
 * Master de la campaña, muestra las 4 pestañas de gestión
 * (Jugadores, NPCs, Monstruos, Combate). Si es un jugador, muestra
 * la lista de sus propios personajes en esa campaña (puede tener
 * varios, D&D 5e no lo prohíbe), con opción de crear uno nuevo y de
 * activar o desactivar cada uno. Un personaje desactivado sigue
 * siendo suyo (puede reactivarlo cuando quiera) pero deja de
 * aparecer en la pantalla del DM.
 */
@Component({
  selector: 'app-campaign-detail',
  standalone: true,
  imports: [PlayersTabComponent, NpcTabComponent, MonsterTabComponent, CombatTabComponent, PersonajeFormComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (authService.esDungeonMaster()) {
      <div class="page-container">
        <div class="tabs">
          <button
            type="button"
            class="tabs__tab"
            [class.tabs__tab--activo]="pestaniaActiva() === 'jugadores'"
            (click)="pestaniaActiva.set('jugadores')"
          >
            Jugadores
          </button>
          <button
            type="button"
            class="tabs__tab"
            [class.tabs__tab--activo]="pestaniaActiva() === 'npcs'"
            (click)="pestaniaActiva.set('npcs')"
          >
            NPC
          </button>
          <button
            type="button"
            class="tabs__tab"
            [class.tabs__tab--activo]="pestaniaActiva() === 'monstruos'"
            (click)="pestaniaActiva.set('monstruos')"
          >
            Monstruos
          </button>
          <button
            type="button"
            class="tabs__tab"
            [class.tabs__tab--activo]="pestaniaActiva() === 'combate'"
            (click)="pestaniaActiva.set('combate')"
          >
            Combate
          </button>
        </div>

        @switch (pestaniaActiva()) {
          @case ('jugadores') {
            <app-players-tab [campaniaId]="campaniaId()"></app-players-tab>
          }
          @case ('npcs') {
            <app-npc-tab [campaniaId]="campaniaId()"></app-npc-tab>
          }
          @case ('monstruos') {
            <app-monster-tab [campaniaId]="campaniaId()"></app-monster-tab>
          }
          @case ('combate') {
            <app-combat-tab [campaniaId]="campaniaId()"></app-combat-tab>
          }
        }
      </div>
    } @else {
      <div class="page-container">
        <div class="campaign-detail__cabecera">
          <h2 class="campaign-detail__titulo-seccion">Tus personajes en esta campaña</h2>
          <button type="button" class="btn btn--primary" (click)="mostrarFormularioCreacion.set(true)">Crear personaje</button>
        </div>

        @if (cargandoPersonajes()) {
          <p class="empty-state">Cargando tus personajes...</p>
        } @else if (misPersonajes().length === 0) {
          <p class="empty-state">Todavía no tienes ningún personaje en esta campaña. Crea el primero con el botón de arriba.</p>
        } @else {
          <div class="campaign-detail__grid">
            @for (personaje of misPersonajes(); track personaje.id) {
              <div class="campaign-detail__card" [class.campaign-detail__card--inactivo]="!personaje.activo">
                <button type="button" class="campaign-detail__card-cuerpo" (click)="verFicha(personaje.id)">
                  <h3 class="campaign-detail__nombre">{{ personaje.nombrePersonaje }}</h3>
                  <p class="campaign-detail__clase">{{ personaje.clase }} — Nivel {{ personaje.nivel }}</p>
                  <div class="campaign-detail__stats">
                    <span class="stat-figure">PV {{ personaje.puntosVidaActuales }}/{{ personaje.puntosVidaMaximos }}</span>
                    <span class="stat-figure">CA {{ personaje.claseArmadura }}</span>
                    <span class="stat-figure">PP {{ personaje.percepcionPasiva }}</span>
                  </div>
                </button>
                <div class="campaign-detail__card-pie">
                  @if (!personaje.activo) {
                    <span class="badge badge--muted">Inactivo</span>
                  }
                  <button
                    type="button"
                    class="btn btn--ghost btn--sm"
                    [disabled]="cambiandoActivoId() === personaje.id"
                    (click)="alternarActivo(personaje)"
                  >
                    {{ personaje.activo ? 'Desactivar' : 'Activar' }}
                  </button>
                </div>
              </div>
            }
          </div>
        }
      </div>

      @if (mostrarFormularioCreacion()) {
        <app-personaje-form [campaniaId]="campaniaId()" (cerrado)="mostrarFormularioCreacion.set(false)" (creado)="onPersonajeCreado()">
        </app-personaje-form>
      }
    }
  `,
  styleUrl: './campaign-detail.component.scss',
})
export class CampaignDetailComponent implements OnInit {
  readonly campaniaId = input.required<number, string>({ transform: (v) => Number(v) });

  protected readonly authService = inject(AuthService);
  private readonly personajeService = inject(PersonajeService);
  private readonly realtimeService = inject(RealtimeService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly pestaniaActiva = signal<PestaniaDM>('jugadores');

  protected readonly cargandoPersonajes = signal(true);
  protected readonly misPersonajes = signal<PersonajeResumen[]>([]);
  protected readonly mostrarFormularioCreacion = signal(false);
  protected readonly cambiandoActivoId = signal<number | null>(null);

  constructor() {
    this.destroyRef.onDestroy(() => {
      this.realtimeService.salirDeCampania(this.campaniaId());
    });
  }

  ngOnInit(): void {
    this.realtimeService.unirseACampania(this.campaniaId());

    if (!this.authService.esDungeonMaster()) {
      this.cargarMisPersonajes();
    }
  }

  private cargarMisPersonajes(): void {
    this.cargandoPersonajes.set(true);
    this.personajeService.getMisPersonajes(this.campaniaId()).subscribe((personajes) => {
      this.cargandoPersonajes.set(false);
      this.misPersonajes.set(personajes);
    });
  }

  protected verFicha(personajeId: number): void {
    this.router.navigate(['/personajes', personajeId]);
  }

  protected onPersonajeCreado(): void {
    this.mostrarFormularioCreacion.set(false);
    this.cargarMisPersonajes();
  }

  protected alternarActivo(personaje: PersonajeResumen): void {
    this.cambiandoActivoId.set(personaje.id);
    this.personajeService.cambiarActivo(personaje.id, !personaje.activo).subscribe({
      next: () => {
        this.cambiandoActivoId.set(null);
        this.misPersonajes.update((lista) => lista.map((p) => (p.id === personaje.id ? { ...p, activo: !p.activo } : p)));
      },
      error: () => {
        this.cambiandoActivoId.set(null);
      },
    });
  }
}
