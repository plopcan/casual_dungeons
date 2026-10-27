import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { ActualizarPersonajeRequest, PersonajeFicha } from '../../../../core/models';

@Component({
  selector: 'app-general-tab',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="general-tab">
      <div class="field">
        <label class="field__label" for="nombre">Nombre del personaje</label>
        <input
          id="nombre"
          class="input"
          type="text"
          [value]="ficha().nombre"
          [disabled]="soloLectura()"
          (input)="emitir({ nombre: $any($event.target).value })"
        />
      </div>

      <div class="general-tab__fila">
        <div class="field">
          <label class="field__label" for="raza">Raza</label>
          <input
            id="raza"
            class="input"
            type="text"
            [value]="ficha().raza"
            [disabled]="soloLectura()"
            (input)="emitir({ raza: $any($event.target).value })"
          />
        </div>

        <div class="field">
          <label class="field__label" for="clase">Clase</label>
          <input
            id="clase"
            class="input"
            type="text"
            [value]="ficha().clase"
            [disabled]="soloLectura()"
            (input)="emitir({ clase: $any($event.target).value })"
          />
        </div>

        <div class="field">
          <label class="field__label" for="subclase">Subclase</label>
          <input
            id="subclase"
            class="input"
            type="text"
            [value]="ficha().subclase ?? ''"
            [disabled]="soloLectura()"
            (input)="emitir({ subclase: $any($event.target).value || null })"
          />
        </div>
      </div>

      <div class="general-tab__fila">
        <div class="field">
          <label class="field__label" for="trasfondo">Trasfondo</label>
          <input
            id="trasfondo"
            class="input"
            type="text"
            [value]="ficha().trasfondo ?? ''"
            [disabled]="soloLectura()"
            (input)="emitir({ trasfondo: $any($event.target).value || null })"
          />
        </div>

        <div class="field">
          <label class="field__label" for="alineamiento">Alineamiento</label>
          <input
            id="alineamiento"
            class="input"
            type="text"
            placeholder="Ej. Legal bueno"
            [value]="ficha().alineamiento ?? ''"
            [disabled]="soloLectura()"
            (input)="emitir({ alineamiento: $any($event.target).value || null })"
          />
        </div>
      </div>

      <div class="general-tab__fila">
        <div class="field">
          <label class="field__label" for="nivel">Nivel</label>
          <input
            id="nivel"
            class="input"
            type="number"
            min="1"
            max="20"
            [value]="ficha().nivel"
            [disabled]="soloLectura()"
            (input)="emitir({ nivel: aNumero($any($event.target).value, 1, 20) })"
          />
        </div>

        <div class="field">
          <label class="field__label" for="experiencia">Experiencia</label>
          <input
            id="experiencia"
            class="input"
            type="number"
            min="0"
            [value]="ficha().experiencia"
            [disabled]="soloLectura()"
            (input)="emitir({ experiencia: aNumero($any($event.target).value, 0) })"
          />
        </div>
      </div>

      <div class="field">
        <label class="field__label" for="imagenUrl">URL de imagen del personaje</label>
        <input
          id="imagenUrl"
          class="input"
          type="text"
          placeholder="https://..."
          [value]="ficha().imagenUrl ?? ''"
          [disabled]="soloLectura()"
          (input)="emitir({ imagenUrl: $any($event.target).value || null })"
        />
      </div>

      @if (ficha().imagenUrl) {
        <img class="general-tab__preview" [src]="ficha().imagenUrl" [alt]="ficha().nombre" />
      }
    </div>
  `,
  styleUrl: './general-tab.component.scss',
})
export class GeneralTabComponent {
  readonly ficha = input.required<PersonajeFicha>();
  readonly soloLectura = input.required<boolean>();
  readonly cambio = output<Partial<ActualizarPersonajeRequest>>();

  protected emitir(cambios: Partial<ActualizarPersonajeRequest>): void {
    if (this.soloLectura()) return;
    this.cambio.emit(cambios);
  }

  protected aNumero(valorTexto: string, min: number, max?: number): number {
    let valor = Number(valorTexto);
    if (Number.isNaN(valor)) valor = min;
    if (valor < min) valor = min;
    if (max !== undefined && valor > max) valor = max;
    return valor;
  }
}
