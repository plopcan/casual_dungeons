import { ChangeDetectionStrategy, Component, computed, inject, input, output } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CampaniaResumen } from '../../../core/models';
import { ArchivoService } from '../../../core/services/archivo.service';
import { CornerFrameComponent } from '../corner-frame/corner-frame.component';

@Component({
  selector: 'app-campaign-card',
  standalone: true,
  imports: [RouterLink, CornerFrameComponent, DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-corner-frame>
      <a class="campaign-card" [routerLink]="['/campanias', campania().id]">
        <div class="campaign-card__imagen">
          @if (imagenUrlAbsoluta()) {
            <img [src]="imagenUrlAbsoluta()" [alt]="campania().nombre" loading="lazy" />
          } @else {
            <div class="campaign-card__imagen-placeholder" aria-hidden="true">🗺</div>
          }
          @if (campania().esDungeonMaster) {
            <span class="badge badge--gold campaign-card__rol-badge">DM</span>
          }
        </div>

        <div class="campaign-card__body">
          <h3 class="campaign-card__nombre">{{ campania().nombre }}</h3>
          @if (campania().descripcion) {
            <p class="campaign-card__descripcion">{{ campania().descripcion }}</p>
          }

          <div class="campaign-card__fechas">
            <span>Inicio: {{ campania().fechaInicio | date: 'dd/MM/yyyy' }}</span>
            @if (campania().fechaFinalizacion) {
              <span>Fin: {{ campania().fechaFinalizacion | date: 'dd/MM/yyyy' }}</span>
            }
          </div>
        </div>
      </a>

      @if (campania().esDungeonMaster) {
        <div class="campaign-card__acciones">
          <button type="button" class="btn btn--ghost btn--sm" (click)="editar.emit(campania())">Editar</button>
          <button type="button" class="btn btn--danger btn--sm" (click)="eliminar.emit(campania())">Eliminar</button>
        </div>
      }
    </app-corner-frame>
  `,
  styleUrl: './campaign-card.component.scss',
})
export class CampaignCardComponent {
  private readonly archivoService = inject(ArchivoService);

  readonly campania = input.required<CampaniaResumen>();

  readonly editar = output<CampaniaResumen>();
  readonly eliminar = output<CampaniaResumen>();

  // Resuelve la URL relativa devuelta por la subida de archivos
  // (/uploads/...) contra el origen del backend. Sin esto, las
  // imágenes subidas por archivo (no por URL externa) darían 404 al
  // intentar cargarse contra el origen del frontend Angular.
  readonly imagenUrlAbsoluta = computed(() => this.archivoService.urlAbsoluta(this.campania().imagenUrl));
}
