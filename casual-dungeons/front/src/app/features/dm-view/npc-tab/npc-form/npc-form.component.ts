import { ChangeDetectionStrategy, Component, computed, inject, input, output, signal } from '@angular/core';
import { NPC } from '../../../../core/models';
import { NPCService } from '../../../../core/services/npc.service';

interface EstadoFormularioNPC {
  nombre: string;
  imagenUrl: string;
  nivel: number;
  puntosVida: number;
  puntosVidaMaximos: number;
  claseArmadura: number;
  fuerza: number;
  destreza: number;
  constitucion: number;
  inteligencia: number;
  sabiduria: number;
  carisma: number;
  notas: string;
}

function estadoDesdeNPC(npc: NPC | null): EstadoFormularioNPC {
  if (!npc) {
    return {
      nombre: '',
      imagenUrl: '',
      nivel: 1,
      puntosVida: 10,
      puntosVidaMaximos: 10,
      claseArmadura: 10,
      fuerza: 10,
      destreza: 10,
      constitucion: 10,
      inteligencia: 10,
      sabiduria: 10,
      carisma: 10,
      notas: '',
    };
  }

  return {
    nombre: npc.nombre,
    imagenUrl: npc.imagenUrl ?? '',
    nivel: npc.nivel,
    puntosVida: npc.puntosVida,
    puntosVidaMaximos: npc.puntosVidaMaximos,
    claseArmadura: npc.claseArmadura,
    fuerza: npc.fuerza,
    destreza: npc.destreza,
    constitucion: npc.constitucion,
    inteligencia: npc.inteligencia,
    sabiduria: npc.sabiduria,
    carisma: npc.carisma,
    notas: npc.notas ?? '',
  };
}

const ATRIBUTOS: { clave: keyof EstadoFormularioNPC; etiqueta: string }[] = [
  { clave: 'fuerza', etiqueta: 'Fuerza' },
  { clave: 'destreza', etiqueta: 'Destreza' },
  { clave: 'constitucion', etiqueta: 'Constitución' },
  { clave: 'inteligencia', etiqueta: 'Inteligencia' },
  { clave: 'sabiduria', etiqueta: 'Sabiduría' },
  { clave: 'carisma', etiqueta: 'Carisma' },
];

@Component({
  selector: 'app-npc-form',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="dialog-overlay" (click)="cerrado.emit()">
      <div class="dialog npc-form" (click)="$event.stopPropagation()">
        <h2 class="dialog__titulo">{{ npc() ? 'Editar NPC' : 'Crear NPC' }}</h2>

        <div class="field">
          <label class="field__label" for="nombre">Nombre</label>
          <input id="nombre" class="input" type="text" [value]="estado().nombre" (input)="set('nombre', $any($event.target).value)" />
        </div>

        <div class="field">
          <label class="field__label" for="imagenUrl">URL de imagen</label>
          <input id="imagenUrl" class="input" type="text" [value]="estado().imagenUrl" (input)="set('imagenUrl', $any($event.target).value)" />
        </div>

        <div class="npc-form__fila">
          <div class="field">
            <label class="field__label" for="nivel">Nivel</label>
            <input id="nivel" class="input" type="number" min="1" [value]="estado().nivel" (input)="setNumero('nivel', $any($event.target).value)" />
          </div>
          <div class="field">
            <label class="field__label" for="claseArmadura">Clase de armadura</label>
            <input
              id="claseArmadura"
              class="input"
              type="number"
              min="0"
              [value]="estado().claseArmadura"
              (input)="setNumero('claseArmadura', $any($event.target).value)"
            />
          </div>
        </div>

        <div class="npc-form__fila">
          <div class="field">
            <label class="field__label" for="puntosVida">Puntos de vida actuales</label>
            <input
              id="puntosVida"
              class="input"
              type="number"
              min="0"
              [value]="estado().puntosVida"
              (input)="setNumero('puntosVida', $any($event.target).value)"
            />
          </div>
          <div class="field">
            <label class="field__label" for="puntosVidaMaximos">Puntos de vida máximos</label>
            <input
              id="puntosVidaMaximos"
              class="input"
              type="number"
              min="1"
              [value]="estado().puntosVidaMaximos"
              (input)="setNumero('puntosVidaMaximos', $any($event.target).value)"
            />
          </div>
        </div>

        <div class="npc-form__atributos">
          @for (atributo of atributos; track atributo.clave) {
            <div class="field">
              <label class="field__label">{{ atributo.etiqueta }}</label>
              <input
                class="input"
                type="number"
                min="1"
                max="30"
                [value]="estado()[atributo.clave]"
                (input)="setNumero(atributo.clave, $any($event.target).value)"
              />
            </div>
          }
        </div>

        <div class="field">
          <label class="field__label" for="notas">Notas</label>
          <textarea id="notas" class="textarea" [value]="estado().notas" (input)="set('notas', $any($event.target).value)"></textarea>
        </div>

        @if (error()) {
          <p class="field__error">{{ error() }}</p>
        }

        <div class="dialog__acciones">
          <button type="button" class="btn btn--ghost" (click)="cerrado.emit()">Cancelar</button>
          <button type="button" class="btn btn--primary" [disabled]="guardando() || !nombreValido()" (click)="guardar()">
            {{ guardando() ? 'Guardando...' : 'Guardar' }}
          </button>
        </div>
      </div>
    </div>
  `,
  styleUrl: './npc-form.component.scss',
})
export class NpcFormComponent {
  readonly campaniaId = input.required<number>();
  readonly npc = input<NPC | null>(null);
  readonly cerrado = output<void>();
  readonly guardado = output<void>();

  protected readonly atributos = ATRIBUTOS;

  private readonly npcService = inject(NPCService);

  protected readonly estado = signal<EstadoFormularioNPC>(estadoDesdeNPC(this.npc()));
  protected readonly guardando = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly nombreValido = computed(() => this.estado().nombre.trim().length > 0);

  protected set<K extends keyof EstadoFormularioNPC>(campo: K, valor: EstadoFormularioNPC[K]): void {
    this.estado.update((actual) => ({ ...actual, [campo]: valor }));
  }

  protected setNumero(campo: keyof EstadoFormularioNPC, valorTexto: string): void {
    const valor = Number(valorTexto);
    this.set(campo, (Number.isNaN(valor) ? 0 : valor) as never);
  }

  protected guardar(): void {
    if (!this.nombreValido()) return;

    this.guardando.set(true);
    this.error.set(null);

    const valores = this.estado();
    const payload = {
      nombre: valores.nombre,
      imagenUrl: valores.imagenUrl || null,
      nivel: valores.nivel,
      puntosVida: valores.puntosVida,
      puntosVidaMaximos: valores.puntosVidaMaximos,
      claseArmadura: valores.claseArmadura,
      fuerza: valores.fuerza,
      destreza: valores.destreza,
      constitucion: valores.constitucion,
      inteligencia: valores.inteligencia,
      sabiduria: valores.sabiduria,
      carisma: valores.carisma,
      notas: valores.notas || null,
    };

    const npcActual = this.npc();
    const observable = npcActual
      ? this.npcService.editar(npcActual.id, payload)
      : this.npcService.crear(this.campaniaId(), payload);

    observable.subscribe({
      next: () => {
        this.guardando.set(false);
        this.guardado.emit();
      },
      error: () => {
        this.guardando.set(false);
        this.error.set('No se pudo guardar el NPC. Verifica los datos e intenta de nuevo.');
      },
    });
  }
}
