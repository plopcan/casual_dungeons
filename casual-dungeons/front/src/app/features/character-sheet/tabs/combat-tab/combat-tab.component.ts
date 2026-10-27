import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { ActualizarPersonajeRequest, PersonajeFicha } from '../../../../core/models';

@Component({
  selector: 'app-combat-tab',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="combat-tab">
      <div class="combat-tab__pv-section">
        <div class="field">
          <label class="field__label" for="pvActuales">Puntos de vida actuales</label>
          <input
            id="pvActuales"
            class="input stat-figure"
            type="number"
            min="0"
            [value]="ficha().puntosVidaActuales"
            [disabled]="soloLectura()"
            (input)="emitirNumero('puntosVidaActuales', $any($event.target).value, 0)"
          />
        </div>

        <div class="field">
          <label class="field__label" for="pvMaximos">Puntos de vida máximos</label>
          <input
            id="pvMaximos"
            class="input stat-figure"
            type="number"
            min="1"
            [value]="ficha().puntosVidaMaximos"
            [disabled]="soloLectura()"
            (input)="emitirNumero('puntosVidaMaximos', $any($event.target).value, 1)"
          />
        </div>

        <div class="field">
          <label class="field__label" for="pvTemp">Puntos de vida temporales</label>
          <input
            id="pvTemp"
            class="input stat-figure"
            type="number"
            min="0"
            [value]="ficha().puntosVidaTemporales"
            [disabled]="soloLectura()"
            (input)="emitirNumero('puntosVidaTemporales', $any($event.target).value, 0)"
          />
        </div>
      </div>

      <div class="combat-tab__grid">
        <div class="field">
          <label class="field__label" for="ca">Clase de armadura</label>
          <input
            id="ca"
            class="input stat-figure"
            type="number"
            min="0"
            [value]="ficha().claseArmadura"
            [disabled]="soloLectura()"
            (input)="emitirNumero('claseArmadura', $any($event.target).value, 0)"
          />
        </div>

        <div class="field">
          <label class="field__label" for="velocidad">Velocidad (pies)</label>
          <input
            id="velocidad"
            class="input stat-figure"
            type="number"
            min="0"
            [value]="ficha().velocidad"
            [disabled]="soloLectura()"
            (input)="emitirNumero('velocidad', $any($event.target).value, 0)"
          />
        </div>

        <div class="field">
          <label class="field__label" for="iniciativa">Bonificador de iniciativa</label>
          <input
            id="iniciativa"
            class="input stat-figure"
            type="number"
            [value]="ficha().iniciativaBonus"
            [disabled]="soloLectura()"
            (input)="emitirNumero('iniciativaBonus', $any($event.target).value)"
          />
        </div>

        <div class="field">
          <label class="field__label">Percepción pasiva</label>
          <div class="input stat-figure combat-tab__readonly">{{ ficha().percepcionPasiva }}</div>
        </div>
      </div>

      <div class="combat-tab__grid">
        <div class="field">
          <label class="field__label" for="dadoGolpe">Dado de golpe</label>
          <input
            id="dadoGolpe"
            class="input"
            type="text"
            placeholder="Ej. d8"
            [value]="ficha().dadoGolpe ?? ''"
            [disabled]="soloLectura()"
            (input)="emitir({ dadoGolpe: $any($event.target).value || null })"
          />
        </div>

        <div class="field">
          <label class="field__label" for="dadosRestantes">Dados de golpe restantes</label>
          <input
            id="dadosRestantes"
            class="input stat-figure"
            type="number"
            min="0"
            [value]="ficha().dadosGolpeRestantes"
            [disabled]="soloLectura()"
            (input)="emitirNumero('dadosGolpeRestantes', $any($event.target).value, 0)"
          />
        </div>
      </div>
    </div>
  `,
  styleUrl: './combat-tab.component.scss',
})
export class CombatTabComponent {
  readonly ficha = input.required<PersonajeFicha>();
  readonly soloLectura = input.required<boolean>();
  readonly cambio = output<Partial<ActualizarPersonajeRequest>>();

  protected emitir(cambios: Partial<ActualizarPersonajeRequest>): void {
    if (this.soloLectura()) return;
    this.cambio.emit(cambios);
  }

  protected emitirNumero(
    campo: keyof ActualizarPersonajeRequest,
    valorTexto: string,
    min?: number,
  ): void {
    if (this.soloLectura()) return;

    let valor = Number(valorTexto);
    if (Number.isNaN(valor)) valor = min ?? 0;
    if (min !== undefined && valor < min) valor = min;

    this.cambio.emit({ [campo]: valor } as Partial<ActualizarPersonajeRequest>);
  }
}
