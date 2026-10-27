import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/**
 * Envuelve su contenido con una orla decorativa de esquinas estilo
 * iluminación medieval. Es el elemento de firma visual del sistema
 * de diseño "Casual Dungeons": dibujado en SVG/CSS puro, sin
 * imágenes externas, para mantenerlo nítido en cualquier resolución
 * y coherente con el tema de color activo (oscuro/claro).
 *
 * Uso:
 *   <app-corner-frame>
 *     <div class="mi-contenido">...</div>
 *   </app-corner-frame>
 */
@Component({
  selector: 'app-corner-frame',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="corner-frame" [class.corner-frame--compact]="compact()">
      <svg class="corner corner--tl" viewBox="0 0 32 32" aria-hidden="true">
        <path d="M2 30 V10 Q2 2 10 2 H30" />
        <circle cx="10" cy="10" r="2" />
      </svg>
      <svg class="corner corner--tr" viewBox="0 0 32 32" aria-hidden="true">
        <path d="M2 2 H22 Q30 2 30 10 V30" />
        <circle cx="22" cy="10" r="2" />
      </svg>
      <svg class="corner corner--bl" viewBox="0 0 32 32" aria-hidden="true">
        <path d="M30 2 V22 Q30 30 22 30 H2" />
        <circle cx="10" cy="22" r="2" />
      </svg>
      <svg class="corner corner--br" viewBox="0 0 32 32" aria-hidden="true">
        <path d="M30 30 H10 Q2 30 2 22 V2" />
        <circle cx="22" cy="22" r="2" />
      </svg>

      <div class="corner-frame__content">
        <ng-content></ng-content>
      </div>
    </div>
  `,
  styleUrl: './corner-frame.component.scss',
})
export class CornerFrameComponent {
  /** Reduce el tamaño de las esquinas para tarjetas pequeñas. */
  readonly compact = input(false);
}
