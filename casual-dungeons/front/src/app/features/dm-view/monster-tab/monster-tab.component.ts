import { httpResource } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, input, signal } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { MonstruoCampania } from '../../../core/models';
import { MonstruoCampaniaService } from '../../../core/services/monstruo-campania.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog.service';
import { CornerFrameComponent } from '../../../shared/components/corner-frame/corner-frame.component';
import { MonstruoCampaniaFormComponent } from './monstruo-campania-form/monstruo-campania-form.component';
import { MonstruoSelectorComponent } from './monstruo-selector/monstruo-selector.component';

@Component({
  selector: 'app-monster-tab',
  standalone: true,
  imports: [CornerFrameComponent, MonstruoCampaniaFormComponent, MonstruoSelectorComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="monster-tab__header">
      <h2>Monstruos de la campaña</h2>
      <div class="monster-tab__acciones-header">
        <button type="button" class="btn btn--ghost" (click)="abrirSelectorCatalogo()">Añadir desde catálogo</button>
        <button type="button" class="btn btn--primary" (click)="abrirCreacion()">+ Crear Monstruo</button>
      </div>
    </div>

    @if (monstruosResource.isLoading()) {
      <p class="empty-state">Cargando monstruos...</p>
    } @else if (monstruosResource.value()?.length === 0) {
      <p class="empty-state">Todavía no has creado ningún monstruo para esta campaña.</p>
    } @else {
      <div class="monster-tab__grid">
        @for (monstruo of monstruosResource.value(); track monstruo.id) {
          <app-corner-frame [compact]="true">
            <div class="monster-card">
              <div class="monster-card__imagen">
                @if (monstruo.imagenUrl) {
                  <img [src]="monstruo.imagenUrl" [alt]="monstruo.nombre" loading="lazy" />
                } @else {
                  <span aria-hidden="true">🐉</span>
                }
              </div>
              <div class="monster-card__body">
                <h3>{{ monstruo.nombre }}</h3>
                <p class="monster-card__tipo">{{ monstruo.tamano }} {{ monstruo.tipo }}</p>
                <div class="monster-card__stats">
                  <span class="stat-figure">PV {{ monstruo.puntosVidaActuales }}/{{ monstruo.puntosVidaMaximos }}</span>
                  <span class="stat-figure">CA {{ monstruo.claseArmadura }}</span>
                  @if (monstruo.desafio) {
                    <span class="stat-figure">Desafío {{ monstruo.desafio }}</span>
                  }
                </div>
              </div>
              <div class="monster-card__acciones">
                <button type="button" class="btn btn--ghost btn--sm" (click)="abrirEdicion(monstruo)">Editar</button>
                <button type="button" class="btn btn--danger btn--sm" (click)="confirmarEliminacion(monstruo)">Eliminar</button>
              </div>
            </div>
          </app-corner-frame>
        }
      </div>
    }

    @if (mostrarFormulario()) {
      <app-monstruo-campania-form
        [campaniaId]="campaniaId()"
        [monstruo]="monstruoEditando()"
        (cerrado)="cerrarFormulario()"
        (guardado)="onGuardado()"
      ></app-monstruo-campania-form>
    }

    @if (mostrarSelectorCatalogo()) {
      <app-monstruo-selector
        [campaniaId]="campaniaId()"
        (cerrado)="mostrarSelectorCatalogo.set(false)"
        (creado)="onCreadoDesdeCatalogo()"
      ></app-monstruo-selector>
    }
  `,
  styleUrl: './monster-tab.component.scss',
})
export class MonsterTabComponent {
  readonly campaniaId = input.required<number>();

  private readonly monstruoCampaniaService = inject(MonstruoCampaniaService);
  private readonly confirmDialog = inject(ConfirmDialogService);

  protected readonly mostrarFormulario = signal(false);
  protected readonly monstruoEditando = signal<MonstruoCampania | null>(null);
  protected readonly mostrarSelectorCatalogo = signal(false);

  protected readonly monstruosResource = httpResource<MonstruoCampania[]>(() => ({
    url: `${environment.apiUrl}/campanias/${this.campaniaId()}/monstruos-campania`,
  }));

  protected abrirCreacion(): void {
    this.monstruoEditando.set(null);
    this.mostrarFormulario.set(true);
  }

  protected abrirSelectorCatalogo(): void {
    this.mostrarSelectorCatalogo.set(true);
  }

  protected onCreadoDesdeCatalogo(): void {
    this.mostrarSelectorCatalogo.set(false);
    this.monstruosResource.reload();
  }

  protected abrirEdicion(monstruo: MonstruoCampania): void {
    this.monstruoEditando.set(monstruo);
    this.mostrarFormulario.set(true);
  }

  protected cerrarFormulario(): void {
    this.mostrarFormulario.set(false);
  }

  protected onGuardado(): void {
    this.mostrarFormulario.set(false);
    this.monstruosResource.reload();
  }

  protected async confirmarEliminacion(monstruo: MonstruoCampania): Promise<void> {
    const confirmado = await this.confirmDialog.confirmar({
      titulo: 'Eliminar monstruo',
      mensaje: `¿Seguro que quieres eliminar a "${monstruo.nombre}"? Esta acción no se puede deshacer.`,
      textoConfirmar: 'Eliminar',
      peligroso: true,
    });

    if (!confirmado) return;

    this.monstruoCampaniaService.eliminar(monstruo.id).subscribe({
      next: () => this.monstruosResource.reload(),
    });
  }
}
