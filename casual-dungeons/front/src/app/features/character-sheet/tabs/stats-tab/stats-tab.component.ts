import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { ActualizarPersonajeRequest, PersonajeFicha } from '../../../../core/models';
import { StatBoxComponent } from '../../../../shared/components/stat-box/stat-box.component';

interface AtributoConfig {
  clave: 'fuerza' | 'destreza' | 'constitucion' | 'inteligencia' | 'sabiduria' | 'carisma';
  claveModificador:
    | 'modificadorFuerza'
    | 'modificadorDestreza'
    | 'modificadorConstitucion'
    | 'modificadorInteligencia'
    | 'modificadorSabiduria'
    | 'modificadorCarisma';
  etiqueta: string;
}

const ATRIBUTOS: AtributoConfig[] = [
  { clave: 'fuerza', claveModificador: 'modificadorFuerza', etiqueta: 'Fuerza' },
  { clave: 'destreza', claveModificador: 'modificadorDestreza', etiqueta: 'Destreza' },
  { clave: 'constitucion', claveModificador: 'modificadorConstitucion', etiqueta: 'Constitución' },
  { clave: 'inteligencia', claveModificador: 'modificadorInteligencia', etiqueta: 'Inteligencia' },
  { clave: 'sabiduria', claveModificador: 'modificadorSabiduria', etiqueta: 'Sabiduría' },
  { clave: 'carisma', claveModificador: 'modificadorCarisma', etiqueta: 'Carisma' },
];

@Component({
  selector: 'app-stats-tab',
  standalone: true,
  imports: [StatBoxComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="stats-tab">
      <div class="stats-tab__resumen">
        <span class="badge badge--gold">Bonificador de competencia: +{{ ficha().bonificadorCompetencia }}</span>
        <span class="badge badge--muted">Percepción pasiva: {{ ficha().percepcionPasiva }}</span>
      </div>

      <div class="stats-tab__grid">
        @for (atributo of atributos; track atributo.clave) {
          <div class="stats-tab__atributo">
            <app-stat-box
              [etiqueta]="atributo.etiqueta"
              [valor]="ficha()[atributo.clave]"
              [modificador]="ficha()[atributo.claveModificador]"
            ></app-stat-box>
            @if (!soloLectura()) {
              <input
                class="input stats-tab__input"
                type="number"
                min="1"
                max="30"
                [value]="ficha()[atributo.clave]"
                (input)="emitirCambioAtributo(atributo.clave, $any($event.target).value)"
              />
            }
          </div>
        }
      </div>
    </div>
  `,
  styleUrl: './stats-tab.component.scss',
})
export class StatsTabComponent {
  readonly ficha = input.required<PersonajeFicha>();
  readonly soloLectura = input.required<boolean>();
  readonly cambio = output<Partial<ActualizarPersonajeRequest>>();

  protected readonly atributos = ATRIBUTOS;

  protected emitirCambioAtributo(clave: AtributoConfig['clave'], valorTexto: string): void {
    if (this.soloLectura()) return;

    let valor = Number(valorTexto);
    if (Number.isNaN(valor)) valor = 1;
    if (valor < 1) valor = 1;
    if (valor > 30) valor = 30;

    this.cambio.emit({ [clave]: valor } as Partial<ActualizarPersonajeRequest>);
  }
}
