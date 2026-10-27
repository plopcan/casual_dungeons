import { httpResource } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, input, signal } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { NPC } from '../../../core/models';
import { NPCService } from '../../../core/services/npc.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog.service';
import { CornerFrameComponent } from '../../../shared/components/corner-frame/corner-frame.component';
import { NpcFormComponent } from './npc-form/npc-form.component';

@Component({
  selector: 'app-npc-tab',
  standalone: true,
  imports: [CornerFrameComponent, NpcFormComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="npc-tab__header">
      <h2>NPCs de la campaña</h2>
      <div class="npc-tab__acciones-header">
        <button type="button" class="btn btn--primary" (click)="abrirCreacion()">+ Crear NPC</button>
      </div>
    </div>

    @if (npcsResource.isLoading()) {
      <p class="empty-state">Cargando NPCs...</p>
    } @else if (npcsResource.value()?.length === 0) {
      <p class="empty-state">Todavía no has creado ningún NPC para esta campaña.</p>
    } @else {
      <div class="npc-tab__grid">
        @for (npc of npcsResource.value(); track npc.id) {
          <app-corner-frame [compact]="true">
            <div class="npc-card">
              <div class="npc-card__imagen">
                @if (npc.imagenUrl) {
                  <img [src]="npc.imagenUrl" [alt]="npc.nombre" loading="lazy" />
                } @else {
                  <span aria-hidden="true">👤</span>
                }
              </div>
              <div class="npc-card__body">
                <h3>{{ npc.nombre }}</h3>
                <p class="npc-card__nivel">Nivel {{ npc.nivel }}</p>
                <div class="npc-card__stats">
                  <span class="stat-figure">PV {{ npc.puntosVida }}/{{ npc.puntosVidaMaximos }}</span>
                  <span class="stat-figure">CA {{ npc.claseArmadura }}</span>
                </div>
              </div>
              <div class="npc-card__acciones">
                <button type="button" class="btn btn--ghost btn--sm" (click)="abrirEdicion(npc)">Editar</button>
                <button type="button" class="btn btn--danger btn--sm" (click)="confirmarEliminacion(npc)">Eliminar</button>
              </div>
            </div>
          </app-corner-frame>
        }
      </div>
    }

    @if (mostrarFormulario()) {
      <app-npc-form
        [campaniaId]="campaniaId()"
        [npc]="npcEditando()"
        (cerrado)="cerrarFormulario()"
        (guardado)="onGuardado()"
      ></app-npc-form>
    }
  `,
  styleUrl: './npc-tab.component.scss',
})
export class NpcTabComponent {
  readonly campaniaId = input.required<number>();

  private readonly npcService = inject(NPCService);
  private readonly confirmDialog = inject(ConfirmDialogService);

  protected readonly mostrarFormulario = signal(false);
  protected readonly npcEditando = signal<NPC | null>(null);

  protected readonly npcsResource = httpResource<NPC[]>(() => ({
    url: `${environment.apiUrl}/campanias/${this.campaniaId()}/npcs`,
  }));

  protected abrirCreacion(): void {
    this.npcEditando.set(null);
    this.mostrarFormulario.set(true);
  }

  protected abrirEdicion(npc: NPC): void {
    this.npcEditando.set(npc);
    this.mostrarFormulario.set(true);
  }

  protected cerrarFormulario(): void {
    this.mostrarFormulario.set(false);
  }

  protected onGuardado(): void {
    this.mostrarFormulario.set(false);
    this.npcsResource.reload();
  }

  protected async confirmarEliminacion(npc: NPC): Promise<void> {
    const confirmado = await this.confirmDialog.confirmar({
      titulo: 'Eliminar NPC',
      mensaje: `¿Seguro que quieres eliminar a "${npc.nombre}"? Esta acción no se puede deshacer.`,
      textoConfirmar: 'Eliminar',
      peligroso: true,
    });

    if (!confirmado) return;

    this.npcService.eliminar(npc.id).subscribe({
      next: () => this.npcsResource.reload(),
    });
  }
}
