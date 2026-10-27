import { ChangeDetectionStrategy, Component, computed, inject, output, signal, input } from '@angular/core';
import { PersonajeService } from '../../../core/services/personaje.service';
import { CrearPersonajeRequest } from '../../../core/models';

interface EstadoFormulario {
  nombre: string;
  raza: string;
  clase: string;
  subclase: string;
  trasfondo: string;
  alineamiento: string;
  fuerza: number;
  destreza: number;
  constitucion: number;
  inteligencia: number;
  sabiduria: number;
  carisma: number;
}

function estadoVacio(): EstadoFormulario {
  return {
    nombre: '',
    raza: '',
    clase: '',
    subclase: '',
    trasfondo: '',
    alineamiento: '',
    fuerza: 10,
    destreza: 10,
    constitucion: 10,
    inteligencia: 10,
    sabiduria: 10,
    carisma: 10,
  };
}

/**
 * Modal de creación de un personaje nuevo (nivel 1). Los puntos de
 * golpe, la clase de armadura, la iniciativa y la percepción pasiva
 * NO se piden aquí: el backend los calcula aplicando las reglas de
 * D&D 5e (PHB) a partir de la clase y los atributos elegidos.
 */
@Component({
  selector: 'app-personaje-form',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="dialog-overlay" (click)="cerrado.emit()">
      <div class="dialog personaje-form" (click)="$event.stopPropagation()">
        <h2 class="dialog__titulo">Crear personaje</h2>

        <div class="personaje-form__fila">
          <div class="field">
            <label class="field__label" for="nombre">Nombre</label>
            <input id="nombre" class="input" type="text" [value]="estado().nombre" (input)="actualizarCampo('nombre', $any($event.target).value)" />
          </div>
          <div class="field">
            <label class="field__label" for="raza">Raza</label>
            <input id="raza" class="input" type="text" [value]="estado().raza" (input)="actualizarCampo('raza', $any($event.target).value)" />
          </div>
        </div>

        <div class="personaje-form__fila">
          <div class="field">
            <label class="field__label" for="clase">Clase</label>
            <input
              id="clase"
              class="input"
              type="text"
              list="clases-5e"
              [value]="estado().clase"
              (input)="actualizarCampo('clase', $any($event.target).value)"
            />
            <datalist id="clases-5e">
              <option value="Bárbaro"></option>
              <option value="Bardo"></option>
              <option value="Brujo"></option>
              <option value="Clérigo"></option>
              <option value="Druida"></option>
              <option value="Explorador"></option>
              <option value="Guerrero"></option>
              <option value="Hechicero"></option>
              <option value="Mago"></option>
              <option value="Monje"></option>
              <option value="Paladín"></option>
              <option value="Pícaro"></option>
            </datalist>
          </div>
          <div class="field">
            <label class="field__label" for="subclase">Subclase (opcional)</label>
            <input id="subclase" class="input" type="text" [value]="estado().subclase" (input)="actualizarCampo('subclase', $any($event.target).value)" />
          </div>
        </div>

        <div class="personaje-form__fila">
          <div class="field">
            <label class="field__label" for="trasfondo">Trasfondo (opcional)</label>
            <input id="trasfondo" class="input" type="text" [value]="estado().trasfondo" (input)="actualizarCampo('trasfondo', $any($event.target).value)" />
          </div>
          <div class="field">
            <label class="field__label" for="alineamiento">Alineamiento (opcional)</label>
            <select
              id="alineamiento"
              class="select"
              [value]="estado().alineamiento"
              (change)="actualizarCampo('alineamiento', $any($event.target).value)"
            >
              <option value="">Sin definir</option>
              <option value="Legal Bueno">Legal Bueno</option>
              <option value="Neutral Bueno">Neutral Bueno</option>
              <option value="Caótico Bueno">Caótico Bueno</option>
              <option value="Legal Neutral">Legal Neutral</option>
              <option value="Neutral">Neutral</option>
              <option value="Caótico Neutral">Caótico Neutral</option>
              <option value="Legal Malvado">Legal Malvado</option>
              <option value="Neutral Malvado">Neutral Malvado</option>
              <option value="Caótico Malvado">Caótico Malvado</option>
            </select>
          </div>
        </div>

        <div class="field">
          <span class="field__label">Atributos</span>
          <p class="personaje-form__ayuda">
            Por defecto 10 (modificador +0) en todos. Los puntos de golpe, la CA, la iniciativa y la percepción pasiva se calculan
            automáticamente según las reglas de D&D 5e.
          </p>
          <div class="personaje-form__atributos">
            <div class="field">
              <label class="field__label" for="fuerza">Fuerza</label>
              <input
                id="fuerza"
                class="input"
                type="number"
                min="1"
                max="30"
                [value]="estado().fuerza"
                (input)="actualizarAtributo('fuerza', $any($event.target).value)"
              />
            </div>
            <div class="field">
              <label class="field__label" for="destreza">Destreza</label>
              <input
                id="destreza"
                class="input"
                type="number"
                min="1"
                max="30"
                [value]="estado().destreza"
                (input)="actualizarAtributo('destreza', $any($event.target).value)"
              />
            </div>
            <div class="field">
              <label class="field__label" for="constitucion">Constitución</label>
              <input
                id="constitucion"
                class="input"
                type="number"
                min="1"
                max="30"
                [value]="estado().constitucion"
                (input)="actualizarAtributo('constitucion', $any($event.target).value)"
              />
            </div>
            <div class="field">
              <label class="field__label" for="inteligencia">Inteligencia</label>
              <input
                id="inteligencia"
                class="input"
                type="number"
                min="1"
                max="30"
                [value]="estado().inteligencia"
                (input)="actualizarAtributo('inteligencia', $any($event.target).value)"
              />
            </div>
            <div class="field">
              <label class="field__label" for="sabiduria">Sabiduría</label>
              <input
                id="sabiduria"
                class="input"
                type="number"
                min="1"
                max="30"
                [value]="estado().sabiduria"
                (input)="actualizarAtributo('sabiduria', $any($event.target).value)"
              />
            </div>
            <div class="field">
              <label class="field__label" for="carisma">Carisma</label>
              <input
                id="carisma"
                class="input"
                type="number"
                min="1"
                max="30"
                [value]="estado().carisma"
                (input)="actualizarAtributo('carisma', $any($event.target).value)"
              />
            </div>
          </div>
        </div>

        @if (errorGuardado()) {
          <p class="field__error">{{ errorGuardado() }}</p>
        }

        <div class="dialog__acciones">
          <button type="button" class="btn btn--ghost" (click)="cerrado.emit()">Cancelar</button>
          <button type="button" class="btn btn--primary" [disabled]="guardando() || !esValido()" (click)="guardar()">
            {{ guardando() ? 'Creando...' : 'Crear personaje' }}
          </button>
        </div>
      </div>
    </div>
  `,
  styleUrl: './personaje-form.component.scss',
})
export class PersonajeFormComponent {
  readonly campaniaId = input.required<number>();
  readonly cerrado = output<void>();
  readonly creado = output<void>();

  private readonly personajeService = inject(PersonajeService);

  protected readonly estado = signal<EstadoFormulario>(estadoVacio());
  protected readonly guardando = signal(false);
  protected readonly errorGuardado = signal<string | null>(null);

  protected readonly esValido = computed(
    () => this.estado().nombre.trim().length > 0 && this.estado().raza.trim().length > 0 && this.estado().clase.trim().length > 0,
  );

  protected actualizarCampo<K extends keyof EstadoFormulario>(campo: K, valor: EstadoFormulario[K]): void {
    this.estado.update((actual) => ({ ...actual, [campo]: valor }));
  }

  // Los atributos llegan como string desde el input[type=number]; se
  // acota entre 1 y 30 aquí mismo para no depender solo de la
  // validación del navegador (que no bloquea valores fuera de rango
  // tecleados manualmente en algunos navegadores).
  protected actualizarAtributo(campo: keyof EstadoFormulario, valorCrudo: string): void {
    const valor = Math.min(30, Math.max(1, Number(valorCrudo) || 10));
    this.estado.update((actual) => ({ ...actual, [campo]: valor }));
  }

  protected guardar(): void {
    if (!this.esValido()) return;

    this.guardando.set(true);
    this.errorGuardado.set(null);

    const valores = this.estado();
    const request: CrearPersonajeRequest = {
      nombre: valores.nombre.trim(),
      raza: valores.raza.trim(),
      clase: valores.clase.trim(),
      subclase: valores.subclase.trim() || null,
      trasfondo: valores.trasfondo.trim() || null,
      alineamiento: valores.alineamiento || null,
      imagenUrl: null,
      fuerza: valores.fuerza,
      destreza: valores.destreza,
      constitucion: valores.constitucion,
      inteligencia: valores.inteligencia,
      sabiduria: valores.sabiduria,
      carisma: valores.carisma,
    };

    this.personajeService.crearPersonaje(this.campaniaId(), request).subscribe({
      next: () => {
        this.guardando.set(false);
        this.creado.emit();
      },
      error: (err) => {
        this.guardando.set(false);
        this.errorGuardado.set(err?.error?.error ?? 'No se pudo crear el personaje. Verifica los datos e intenta de nuevo.');
      },
    });
  }
}
