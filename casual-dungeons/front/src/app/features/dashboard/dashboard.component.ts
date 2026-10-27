import { httpResource } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../core/services/auth.service';
import { CampaniaService } from '../../core/services/campania.service';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { CampaignCardComponent } from '../../shared/components/campaign-card/campaign-card.component';
import { CampaniaResumen } from '../../core/models';
import { CampaignFormComponent } from '../campaigns/campaign-form/campaign-form.component';

type Pestania = 'activas' | 'finalizadas';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CampaignCardComponent, CampaignFormComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-container">
      <div class="dashboard__header">
        <h1>Tus campañas</h1>
        @if (authService.esDungeonMaster()) {
          <button type="button" class="btn btn--primary" (click)="abrirCreacion()">+ Crear campaña</button>
        }
      </div>

      <div class="tabs">
        <button
          type="button"
          class="tabs__tab"
          [class.tabs__tab--activo]="pestaniaActiva() === 'activas'"
          (click)="pestaniaActiva.set('activas')"
        >
          Campañas activas
        </button>
        <button
          type="button"
          class="tabs__tab"
          [class.tabs__tab--activo]="pestaniaActiva() === 'finalizadas'"
          (click)="pestaniaActiva.set('finalizadas')"
        >
          Campañas finalizadas
        </button>
      </div>

      @if (campaniasResource.isLoading()) {
        <p class="empty-state">Cargando campañas...</p>
      } @else if (campaniasResource.error()) {
        <p class="empty-state">No se pudieron cargar las campañas. Intenta recargar la página.</p>
      } @else if (campaniasResource.value()?.length === 0) {
        <p class="empty-state">
          @if (pestaniaActiva() === 'activas') {
            No tienes campañas activas todavía.
          } @else {
            No hay campañas finalizadas.
          }
        </p>
      } @else {
        <div class="dashboard__grid">
          @for (campania of campaniasResource.value(); track campania.id) {
            <app-campaign-card
              [campania]="campania"
              (editar)="abrirEdicion($event)"
              (eliminar)="confirmarEliminacion($event)"
            ></app-campaign-card>
          }
        </div>
      }
    </div>

    @if (mostrarFormulario()) {
      <app-campaign-form
        [campaniaId]="campaniaEditandoId()"
        (cerrado)="cerrarFormulario()"
        (guardado)="onCampaniaGuardada()"
      ></app-campaign-form>
    }
  `,
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent {
  protected readonly authService = inject(AuthService);
  private readonly campaniaService = inject(CampaniaService);
  private readonly confirmDialog = inject(ConfirmDialogService);
  private readonly router = inject(Router);

  protected readonly pestaniaActiva = signal<Pestania>('activas');
  protected readonly mostrarFormulario = signal(false);
  protected readonly campaniaEditandoId = signal<number | null>(null);

  protected readonly campaniasResource = httpResource<CampaniaResumen[]>(() => ({
    url: `${environment.apiUrl}/campanias`,
    params: { activas: this.pestaniaActiva() === 'activas' ? 'true' : 'false' },
  }));

  protected abrirCreacion(): void {
    this.campaniaEditandoId.set(null);
    this.mostrarFormulario.set(true);
  }

  protected abrirEdicion(campania: CampaniaResumen): void {
    this.campaniaEditandoId.set(campania.id);
    this.mostrarFormulario.set(true);
  }

  protected cerrarFormulario(): void {
    this.mostrarFormulario.set(false);
  }

  protected onCampaniaGuardada(): void {
    this.mostrarFormulario.set(false);
    this.campaniasResource.reload();
  }

  protected async confirmarEliminacion(campania: CampaniaResumen): Promise<void> {
    const confirmado = await this.confirmDialog.confirmar({
      titulo: 'Eliminar campaña',
      mensaje: `¿Seguro que quieres eliminar "${campania.nombre}"? Se eliminarán también todos los personajes, NPCs y combates asociados. Esta acción no se puede deshacer.`,
      textoConfirmar: 'Eliminar',
      peligroso: true,
    });

    if (!confirmado) return;

    this.campaniaService.eliminar(campania.id).subscribe({
      next: () => this.campaniasResource.reload(),
    });
  }
}
