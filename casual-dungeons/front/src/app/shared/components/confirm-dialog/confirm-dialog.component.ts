import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ConfirmDialogService } from './confirm-dialog.service';

@Component({
  selector: 'app-confirm-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (service.solicitud(); as solicitud) {
      <div class="dialog-overlay" (click)="service.responder(false)">
        <div class="dialog" [class.dialog--peligroso]="solicitud.peligroso" (click)="$event.stopPropagation()">
          <h3 class="dialog__titulo">{{ solicitud.titulo }}</h3>
          <p class="dialog__mensaje">{{ solicitud.mensaje }}</p>
          <div class="dialog__acciones">
            <button type="button" class="btn btn--ghost" (click)="service.responder(false)">
              {{ solicitud.textoCancelar }}
            </button>
            <button
              type="button"
              class="btn"
              [class.btn--danger]="solicitud.peligroso"
              [class.btn--primary]="!solicitud.peligroso"
              (click)="service.responder(true)"
            >
              {{ solicitud.textoConfirmar }}
            </button>
          </div>
        </div>
      </div>
    }
  `,
  styleUrl: './confirm-dialog.component.scss',
})
export class ConfirmDialogComponent {
  protected readonly service = inject(ConfirmDialogService);
}
