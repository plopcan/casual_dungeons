import { ChangeDetectionStrategy, Component, computed, inject, input, output, signal } from '@angular/core';
import { MonstruoCampania } from '../../../../core/models';
import { MonstruoCampaniaService } from '../../../../core/services/monstruo-campania.service';

interface EstadoFormularioMonstruo {
  nombre: string;
  imagenUrl: string;
  tipo: string;
  tamano: string;
  alineamiento: string;
  claseArmadura: number;
  descripcionArmadura: string;
  puntosVidaMaximos: number;
  puntosVidaActuales: number;
  dadoGolpe: string;
  velocidad: string;
  fuerza: number;
  destreza: number;
  constitucion: number;
  inteligencia: number;
  sabiduria: number;
  carisma: number;
  salvaciones: string;
  habilidades: string;
  resistenciasDano: string;
  inmunidadesDano: string;
  inmunidadesEstado: string;
  sentidos: string;
  idiomas: string;
  desafio: string;
  puntosExperiencia: number | null;
  rasgosAcciones: string;
}

function estadoDesdeMonstruo(monstruo: MonstruoCampania | null): EstadoFormularioMonstruo {
  if (!monstruo) {
    return {
      nombre: '',
      imagenUrl: '',
      tipo: '',
      tamano: 'Mediano',
      alineamiento: '',
      claseArmadura: 10,
      descripcionArmadura: '',
      puntosVidaMaximos: 10,
      puntosVidaActuales: 10,
      dadoGolpe: '',
      velocidad: '9 m',
      fuerza: 10,
      destreza: 10,
      constitucion: 10,
      inteligencia: 10,
      sabiduria: 10,
      carisma: 10,
      salvaciones: '',
      habilidades: '',
      resistenciasDano: '',
      inmunidadesDano: '',
      inmunidadesEstado: '',
      sentidos: '',
      idiomas: '',
      desafio: '',
      puntosExperiencia: null,
      rasgosAcciones: '',
    };
  }

  return {
    nombre: monstruo.nombre,
    imagenUrl: monstruo.imagenUrl ?? '',
    tipo: monstruo.tipo,
    tamano: monstruo.tamano,
    alineamiento: monstruo.alineamiento ?? '',
    claseArmadura: monstruo.claseArmadura,
    descripcionArmadura: monstruo.descripcionArmadura ?? '',
    puntosVidaMaximos: monstruo.puntosVidaMaximos,
    puntosVidaActuales: monstruo.puntosVidaActuales,
    dadoGolpe: monstruo.dadoGolpe ?? '',
    velocidad: monstruo.velocidad ?? '',
    fuerza: monstruo.fuerza,
    destreza: monstruo.destreza,
    constitucion: monstruo.constitucion,
    inteligencia: monstruo.inteligencia,
    sabiduria: monstruo.sabiduria,
    carisma: monstruo.carisma,
    salvaciones: monstruo.salvaciones ?? '',
    habilidades: monstruo.habilidades ?? '',
    resistenciasDano: monstruo.resistenciasDano ?? '',
    inmunidadesDano: monstruo.inmunidadesDano ?? '',
    inmunidadesEstado: monstruo.inmunidadesEstado ?? '',
    sentidos: monstruo.sentidos ?? '',
    idiomas: monstruo.idiomas ?? '',
    desafio: monstruo.desafio ?? '',
    puntosExperiencia: monstruo.puntosExperiencia,
    rasgosAcciones: monstruo.rasgosAcciones ?? '',
  };
}

const ATRIBUTOS: { clave: keyof EstadoFormularioMonstruo; etiqueta: string }[] = [
  { clave: 'fuerza', etiqueta: 'Fuerza' },
  { clave: 'destreza', etiqueta: 'Destreza' },
  { clave: 'constitucion', etiqueta: 'Constitución' },
  { clave: 'inteligencia', etiqueta: 'Inteligencia' },
  { clave: 'sabiduria', etiqueta: 'Sabiduría' },
  { clave: 'carisma', etiqueta: 'Carisma' },
];

@Component({
  selector: 'app-monstruo-campania-form',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="dialog-overlay" (click)="cerrado.emit()">
      <div class="dialog monstruo-campania-form" (click)="$event.stopPropagation()">
        <h2 class="dialog__titulo">{{ monstruo() ? 'Editar monstruo' : 'Crear monstruo' }}</h2>

        <h3 class="monstruo-campania-form__seccion">Datos generales</h3>

        <div class="field">
          <label class="field__label" for="nombre">Nombre</label>
          <input id="nombre" class="input" type="text" [value]="estado().nombre" (input)="set('nombre', $any($event.target).value)" />
        </div>

        <div class="field">
          <label class="field__label" for="imagenUrl">URL de imagen</label>
          <input id="imagenUrl" class="input" type="text" [value]="estado().imagenUrl" (input)="set('imagenUrl', $any($event.target).value)" />
        </div>

        <div class="monstruo-campania-form__fila">
          <div class="field">
            <label class="field__label" for="tipo">Tipo</label>
            <input id="tipo" class="input" type="text" placeholder="Ej. Humanoide (goblinoide)" [value]="estado().tipo" (input)="set('tipo', $any($event.target).value)" />
          </div>
          <div class="field">
            <label class="field__label" for="tamano">Tamaño</label>
            <input id="tamano" class="input" type="text" placeholder="Ej. Mediano" [value]="estado().tamano" (input)="set('tamano', $any($event.target).value)" />
          </div>
          <div class="field">
            <label class="field__label" for="alineamiento">Alineamiento</label>
            <input id="alineamiento" class="input" type="text" [value]="estado().alineamiento" (input)="set('alineamiento', $any($event.target).value)" />
          </div>
        </div>

        <h3 class="monstruo-campania-form__seccion">Combate</h3>

        <div class="monstruo-campania-form__fila">
          <div class="field">
            <label class="field__label" for="claseArmadura">Clase de armadura</label>
            <input id="claseArmadura" class="input" type="number" min="0" [value]="estado().claseArmadura" (input)="setNumero('claseArmadura', $any($event.target).value)" />
          </div>
          <div class="field">
            <label class="field__label" for="descripcionArmadura">Descripción de la armadura</label>
            <input id="descripcionArmadura" class="input" type="text" placeholder="Ej. armadura natural" [value]="estado().descripcionArmadura" (input)="set('descripcionArmadura', $any($event.target).value)" />
          </div>
        </div>

        <div class="monstruo-campania-form__fila">
          <div class="field">
            <label class="field__label" for="puntosVidaActuales">Puntos de vida actuales</label>
            <input id="puntosVidaActuales" class="input" type="number" min="0" [value]="estado().puntosVidaActuales" (input)="setNumero('puntosVidaActuales', $any($event.target).value)" />
          </div>
          <div class="field">
            <label class="field__label" for="puntosVidaMaximos">Puntos de vida máximos</label>
            <input id="puntosVidaMaximos" class="input" type="number" min="1" [value]="estado().puntosVidaMaximos" (input)="setNumero('puntosVidaMaximos', $any($event.target).value)" />
          </div>
          <div class="field">
            <label class="field__label" for="dadoGolpe">Dado de golpe</label>
            <input id="dadoGolpe" class="input" type="text" placeholder="Ej. 2d8+2" [value]="estado().dadoGolpe" (input)="set('dadoGolpe', $any($event.target).value)" />
          </div>
        </div>

        <div class="field">
          <label class="field__label" for="velocidad">Velocidad</label>
          <input id="velocidad" class="input" type="text" placeholder="Ej. 9 m, vuelo 18 m" [value]="estado().velocidad" (input)="set('velocidad', $any($event.target).value)" />
        </div>

        <h3 class="monstruo-campania-form__seccion">Atributos</h3>

        <div class="monstruo-campania-form__atributos">
          @for (atributo of atributos; track atributo.clave) {
            <div class="field">
              <label class="field__label">{{ atributo.etiqueta }}</label>
              <input class="input" type="number" min="1" max="30" [value]="estado()[atributo.clave]" (input)="setNumero(atributo.clave, $any($event.target).value)" />
            </div>
          }
        </div>

        <h3 class="monstruo-campania-form__seccion">Salvaciones, resistencias y sentidos</h3>

        <div class="field">
          <label class="field__label" for="salvaciones">Salvaciones</label>
          <input id="salvaciones" class="input" type="text" placeholder="Ej. Destreza +5, Sabiduría +3" [value]="estado().salvaciones" (input)="set('salvaciones', $any($event.target).value)" />
        </div>

        <div class="field">
          <label class="field__label" for="habilidades">Habilidades</label>
          <input id="habilidades" class="input" type="text" placeholder="Ej. Sigilo +4, Percepción +3" [value]="estado().habilidades" (input)="set('habilidades', $any($event.target).value)" />
        </div>

        <div class="monstruo-campania-form__fila">
          <div class="field">
            <label class="field__label" for="resistenciasDano">Resistencias al daño</label>
            <input id="resistenciasDano" class="input" type="text" [value]="estado().resistenciasDano" (input)="set('resistenciasDano', $any($event.target).value)" />
          </div>
          <div class="field">
            <label class="field__label" for="inmunidadesDano">Inmunidades al daño</label>
            <input id="inmunidadesDano" class="input" type="text" [value]="estado().inmunidadesDano" (input)="set('inmunidadesDano', $any($event.target).value)" />
          </div>
          <div class="field">
            <label class="field__label" for="inmunidadesEstado">Inmunidades a estados</label>
            <input id="inmunidadesEstado" class="input" type="text" [value]="estado().inmunidadesEstado" (input)="set('inmunidadesEstado', $any($event.target).value)" />
          </div>
        </div>

        <div class="monstruo-campania-form__fila">
          <div class="field">
            <label class="field__label" for="sentidos">Sentidos</label>
            <input id="sentidos" class="input" type="text" placeholder="Ej. visión en la oscuridad 18 m, percepción pasiva 12" [value]="estado().sentidos" (input)="set('sentidos', $any($event.target).value)" />
          </div>
          <div class="field">
            <label class="field__label" for="idiomas">Idiomas</label>
            <input id="idiomas" class="input" type="text" [value]="estado().idiomas" (input)="set('idiomas', $any($event.target).value)" />
          </div>
        </div>

        <div class="monstruo-campania-form__fila">
          <div class="field">
            <label class="field__label" for="desafio">Desafío</label>
            <input id="desafio" class="input" type="text" placeholder="Ej. 1/4, 2, 5" [value]="estado().desafio" (input)="set('desafio', $any($event.target).value)" />
          </div>
          <div class="field">
            <label class="field__label" for="puntosExperiencia">Puntos de experiencia</label>
            <input
              id="puntosExperiencia"
              class="input"
              type="number"
              min="0"
              [value]="estado().puntosExperiencia ?? ''"
              (input)="setNumeroOpcional('puntosExperiencia', $any($event.target).value)"
            />
          </div>
        </div>

        <h3 class="monstruo-campania-form__seccion">Rasgos, acciones y reacciones</h3>

        <div class="field">
          <label class="field__label" for="rasgosAcciones">Texto libre (rasgos, acciones, acciones legendarias, reacciones...)</label>
          <textarea
            id="rasgosAcciones"
            class="textarea monstruo-campania-form__textarea-grande"
            [value]="estado().rasgosAcciones"
            (input)="set('rasgosAcciones', $any($event.target).value)"
          ></textarea>
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
  styleUrl: './monstruo-campania-form.component.scss',
})
export class MonstruoCampaniaFormComponent {
  readonly campaniaId = input.required<number>();
  readonly monstruo = input<MonstruoCampania | null>(null);
  readonly cerrado = output<void>();
  readonly guardado = output<void>();

  protected readonly atributos = ATRIBUTOS;

  private readonly monstruoCampaniaService = inject(MonstruoCampaniaService);

  protected readonly estado = signal<EstadoFormularioMonstruo>(estadoDesdeMonstruo(this.monstruo()));
  protected readonly guardando = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly nombreValido = computed(() => this.estado().nombre.trim().length > 0);

  protected set<K extends keyof EstadoFormularioMonstruo>(campo: K, valor: EstadoFormularioMonstruo[K]): void {
    this.estado.update((actual) => ({ ...actual, [campo]: valor }));
  }

  protected setNumero(campo: keyof EstadoFormularioMonstruo, valorTexto: string): void {
    const valor = Number(valorTexto);
    this.set(campo, (Number.isNaN(valor) ? 0 : valor) as never);
  }

  protected setNumeroOpcional(campo: keyof EstadoFormularioMonstruo, valorTexto: string): void {
    if (valorTexto.trim() === '') {
      this.set(campo, null as never);
      return;
    }
    const valor = Number(valorTexto);
    this.set(campo, (Number.isNaN(valor) ? null : valor) as never);
  }

  protected guardar(): void {
    if (!this.nombreValido()) return;

    this.guardando.set(true);
    this.error.set(null);

    const v = this.estado();
    const payload = {
      nombre: v.nombre,
      imagenUrl: v.imagenUrl || null,
      tipo: v.tipo || 'Desconocido',
      tamano: v.tamano || 'Mediano',
      alineamiento: v.alineamiento || null,
      claseArmadura: v.claseArmadura,
      descripcionArmadura: v.descripcionArmadura || null,
      puntosVidaMaximos: v.puntosVidaMaximos,
      puntosVidaActuales: v.puntosVidaActuales,
      dadoGolpe: v.dadoGolpe || null,
      velocidad: v.velocidad || null,
      fuerza: v.fuerza,
      destreza: v.destreza,
      constitucion: v.constitucion,
      inteligencia: v.inteligencia,
      sabiduria: v.sabiduria,
      carisma: v.carisma,
      salvaciones: v.salvaciones || null,
      habilidades: v.habilidades || null,
      resistenciasDano: v.resistenciasDano || null,
      inmunidadesDano: v.inmunidadesDano || null,
      inmunidadesEstado: v.inmunidadesEstado || null,
      sentidos: v.sentidos || null,
      idiomas: v.idiomas || null,
      desafio: v.desafio || null,
      puntosExperiencia: v.puntosExperiencia,
      rasgosAcciones: v.rasgosAcciones || null,
    };

    const monstruoActual = this.monstruo();
    const observable = monstruoActual
      ? this.monstruoCampaniaService.editar(monstruoActual.id, payload)
      : this.monstruoCampaniaService.crear(this.campaniaId(), payload);

    observable.subscribe({
      next: () => {
        this.guardando.set(false);
        this.guardado.emit();
      },
      error: () => {
        this.guardando.set(false);
        this.error.set('No se pudo guardar el monstruo. Verifica los datos e intenta de nuevo.');
      },
    });
  }
}
