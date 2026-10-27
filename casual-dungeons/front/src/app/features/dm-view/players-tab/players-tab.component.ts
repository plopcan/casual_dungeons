import { httpResource } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, effect, inject, input } from '@angular/core';
import { Router } from '@angular/router';
import { environment } from '../../../../environments/environment';
import { PersonajeResumen } from '../../../core/models';
import { RealtimeService } from '../../../core/services/realtime.service';

@Component({
  selector: 'app-players-tab',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (personajesResource.isLoading()) {
      <p class="empty-state">Cargando personajes...</p>
    } @else if (personajesResource.value()?.length === 0) {
      <p class="empty-state">Todavía no hay personajes creados en esta campaña.</p>
    } @else {
      <div class="players-tab__grid">
        @for (personaje of personajesResource.value(); track personaje.id) {
          <button type="button" class="players-tab__card" (click)="verFicha(personaje.id)">
            <h3 class="players-tab__nombre">{{ personaje.nombrePersonaje }}</h3>
            <p class="players-tab__jugador">{{ personaje.nombreJugador }}</p>
            <p class="players-tab__clase">{{ personaje.clase }} — Nivel {{ personaje.nivel }}</p>

            <div class="players-tab__stats">
              <span class="stat-figure">PV {{ personaje.puntosVidaActuales }}/{{ personaje.puntosVidaMaximos }}</span>
              <span class="stat-figure">CA {{ personaje.claseArmadura }}</span>
              <span class="stat-figure">PP {{ personaje.percepcionPasiva }}</span>
            </div>
          </button>
        }
      </div>
    }
  `,
  styleUrl: './players-tab.component.scss',
})
export class PlayersTabComponent {
  readonly campaniaId = input.required<number>();

  private readonly router = inject(Router);
  private readonly realtimeService = inject(RealtimeService);

  protected readonly personajesResource = httpResource<PersonajeResumen[]>(() => ({
    url: `${environment.apiUrl}/campanias/${this.campaniaId()}/personajes`,
  }));

  constructor() {
    // Cuando SignalR notifica que un personaje cambió, se recarga
    // el listado para reflejar PV/CA actualizados sin recargar la página.
    effect(() => {
      if (this.realtimeService.personajeActualizado() !== null) {
        this.personajesResource.reload();
      }
    });
  }

  protected verFicha(personajeId: number): void {
    this.router.navigate(['/personajes', personajeId]);
  }
}
