import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

/**
 * Muestra un atributo D&D (ej. Fuerza: 16, modificador +3) en un
 * recuadro compacto. El modificador se formatea siempre con signo
 * explícito (+3 / -1 / +0), como es convención en las fichas de
 * personaje impresas.
 */
@Component({
  selector: 'app-stat-box',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="stat-box">
      <span class="stat-box__label">{{ etiqueta() }}</span>
      <span class="stat-box__valor stat-figure">{{ valor() }}</span>
      <span class="stat-box__modificador stat-figure">{{ modificadorFormateado() }}</span>
    </div>
  `,
  styleUrl: './stat-box.component.scss',
})
export class StatBoxComponent {
  readonly etiqueta = input.required<string>();
  readonly valor = input.required<number>();
  readonly modificador = input.required<number>();

  readonly modificadorFormateado = computed(() => {
    const mod = this.modificador();
    return mod >= 0 ? `+${mod}` : `${mod}`;
  });
}
