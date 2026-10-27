import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, effect, inject, input, signal } from '@angular/core';
import { ActualizarPersonajeRequest, PersonajeFicha } from '../../core/models';
import { PersonajeService } from '../../core/services/personaje.service';
import { RealtimeService } from '../../core/services/realtime.service';
import { GeneralTabComponent } from './tabs/general-tab/general-tab.component';
import { StatsTabComponent } from './tabs/stats-tab/stats-tab.component';
import { CombatTabComponent as CharacterCombatTabComponent } from './tabs/combat-tab/combat-tab.component';
import { InventoryTabComponent } from './tabs/inventory-tab/inventory-tab.component';
import { SkillsTabComponent } from './tabs/skills-tab/skills-tab.component';
import { SpellsTabComponent } from './tabs/spells-tab/spells-tab.component';
import { NotesTabComponent } from './tabs/notes-tab/notes-tab.component';

type PestaniaFicha = 'general' | 'estadisticas' | 'combate' | 'inventario' | 'habilidades' | 'hechizos' | 'notas';

const DEBOUNCE_AUTOGUARDADO_MS = 800;

/**
 * Replica Personaje.CalcularModificador del backend (Domain):
 * floor((valor - 10) / 2). Se usa Math.floor explícito porque la
 * división de JS con negativos no redondea hacia -infinito por
 * defecto en todos los casos relevantes (ej. -3/2 en JS da -1.5,
 * Math.floor(-1.5) = -2, que es el comportamiento correcto buscado).
 */
function calcularModificador(valorAtributo: number): number {
  return Math.floor((valorAtributo - 10) / 2);
}

/**
 * Replica Personaje.BonificadorCompetencia del backend:
 * 2 + floor((nivel - 1) / 4).
 */
function calcularBonificadorCompetencia(nivel: number): number {
  return 2 + Math.floor((nivel - 1) / 4);
}

/**
 * Conjunto de campos editables de la ficha que disparan autoguardado.
 * Coincide con ActualizarPersonajeRequest del backend.
 */
type CamposEditables = Omit<ActualizarPersonajeRequest, never>;

@Component({
  selector: 'app-character-sheet',
  standalone: true,
  imports: [
    GeneralTabComponent,
    StatsTabComponent,
    CharacterCombatTabComponent,
    InventoryTabComponent,
    SkillsTabComponent,
    SpellsTabComponent,
    NotesTabComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (ficha(); as f) {
      <div class="page-container character-sheet">
        <header class="character-sheet__header">
          <div>
            <h1>{{ f.nombre }}</h1>
            <p class="character-sheet__subtitulo">
              {{ f.raza }} · {{ f.clase }}@if (f.subclase) {
                ({{ f.subclase }})
              }
              · Nivel {{ f.nivel }}
            </p>
          </div>

          <div class="character-sheet__estado-guardado">
            @if (guardando()) {
              <span class="badge badge--muted">Guardando...</span>
            } @else if (ultimoGuardadoOk()) {
              <span class="badge badge--success">Guardado</span>
            }
            @if (f.soloLectura) {
              <span class="badge badge--gold">Solo lectura</span>
            }
          </div>
        </header>

        <div class="tabs character-sheet__tabs">
          @for (pestania of pestanias; track pestania.id) {
            <button
              type="button"
              class="tabs__tab"
              [class.tabs__tab--activo]="pestaniaActiva() === pestania.id"
              (click)="pestaniaActiva.set(pestania.id)"
            >
              {{ pestania.etiqueta }}
            </button>
          }
        </div>

        @switch (pestaniaActiva()) {
          @case ('general') {
            <app-general-tab [ficha]="f" [soloLectura]="f.soloLectura" (cambio)="actualizarCampos($event)"></app-general-tab>
          }
          @case ('estadisticas') {
            <app-stats-tab [ficha]="f" [soloLectura]="f.soloLectura" (cambio)="actualizarCampos($event)"></app-stats-tab>
          }
          @case ('combate') {
            <app-combat-tab [ficha]="f" [soloLectura]="f.soloLectura" (cambio)="actualizarCampos($event)"></app-combat-tab>
          }
          @case ('inventario') {
            <app-inventory-tab [personajeId]="f.id" [items]="f.inventario" [soloLectura]="f.soloLectura" (cambio)="recargarFicha()"></app-inventory-tab>
          }
          @case ('habilidades') {
            <app-skills-tab [personajeId]="f.id" [habilidades]="f.habilidades" [soloLectura]="f.soloLectura" (cambio)="recargarFicha()"></app-skills-tab>
          }
          @case ('hechizos') {
            <app-spells-tab
              [personajeId]="f.id"
              [hechizos]="f.hechizos"
              [espaciosConjuro]="f.espaciosConjuro"
              [soloLectura]="f.soloLectura"
              (cambio)="recargarFicha()"
            ></app-spells-tab>
          }
          @case ('notas') {
            <app-notes-tab [personajeId]="f.id" [notas]="f.notas" [soloLectura]="f.soloLectura" (cambio)="recargarFicha()"></app-notes-tab>
          }
        }
      </div>
    } @else {
      <p class="empty-state">Cargando ficha de personaje...</p>
    }
  `,
  styleUrl: './character-sheet.component.scss',
})
export class CharacterSheetComponent implements OnInit {
  readonly personajeId = input.required<number, string>({ transform: (v) => Number(v) });

  private readonly personajeService = inject(PersonajeService);
  private readonly realtimeService = inject(RealtimeService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly pestanias: { id: PestaniaFicha; etiqueta: string }[] = [
    { id: 'general', etiqueta: 'General' },
    { id: 'estadisticas', etiqueta: 'Estadísticas' },
    { id: 'combate', etiqueta: 'Combate' },
    { id: 'inventario', etiqueta: 'Inventario y equipo' },
    { id: 'habilidades', etiqueta: 'Habilidades' },
    { id: 'hechizos', etiqueta: 'Hechizos' },
    { id: 'notas', etiqueta: 'Notas' },
  ];

  protected readonly pestaniaActiva = signal<PestaniaFicha>('general');
  protected readonly ficha = signal<PersonajeFicha | null>(null);
  protected readonly guardando = signal(false);
  protected readonly ultimoGuardadoOk = signal(false);

  private debounceTimer: ReturnType<typeof setTimeout> | null = null;
  private pendienteDeGuardar: Partial<CamposEditables> | null = null;

  /** Campaña a la que esta pestaña está unida en el grupo de SignalR (ver cargarFicha). */
  private campaniaUnida: number | null = null;

  constructor() {
    // OJO: this.personajeId() (input.required) todavía no tiene valor
    // aquí dentro del constructor -- Angular lo asigna después de
    // instanciar el componente. Por eso cargarFicha() se llama desde
    // ngOnInit() más abajo, no aquí. El effect() sí es seguro en el
    // constructor porque su primera ejecución se difiere hasta
    // después de que los inputs estén asignados.
    effect(() => {
      const idActualizado = this.realtimeService.personajeActualizado();
      if (idActualizado !== null && idActualizado === this.personajeId() && this.ficha()?.soloLectura) {
        // Solo recargamos automáticamente en modo solo lectura (vista
        // del DM); si el propio jugador está editando, no queremos
        // pisar su edición en curso con una recarga no solicitada.
        this.recargarFicha();
      }
    });

    this.destroyRef.onDestroy(() => {
      if (this.debounceTimer) {
        clearTimeout(this.debounceTimer);
        this.guardarInmediatamente();
      }

      // La ficha de personaje es una ruta de nivel superior
      // (/personajes/:id), independiente de la vista de campaña
      // (/campanias/:id), así que es responsabilidad de este propio
      // componente unirse y salir del grupo de SignalR; si no, esta
      // pestaña nunca recibe PersonajeActualizado/NPCActualizado y
      // hay que recargar a mano para ver cambios hechos en otra
      // pestaña o por otro usuario conectado a la misma campaña.
      if (this.campaniaUnida !== null) {
        this.realtimeService.salirDeCampania(this.campaniaUnida);
      }
    });
  }

  ngOnInit(): void {
    this.cargarFicha();
  }

  private cargarFicha(): void {
    this.personajeService.getFicha(this.personajeId()).subscribe((ficha) => {
      this.ficha.set(ficha);
      this.unirseACampaniaSiHaceFalta(ficha.campaniaId);
    });
  }

  /**
   * Une esta pestaña al grupo de SignalR de la campaña del
   * personaje la primera vez que se conoce su campaniaId. No hace
   * falta repetirlo en cada recargarFicha() mientras sea la misma
   * campaña (por ejemplo tras guardar inventario/hechizos/notas).
   */
  private unirseACampaniaSiHaceFalta(campaniaId: number): void {
    if (this.campaniaUnida === campaniaId) return;

    if (this.campaniaUnida !== null) {
      this.realtimeService.salirDeCampania(this.campaniaUnida);
    }

    this.campaniaUnida = campaniaId;
    this.realtimeService.unirseACampania(campaniaId);
  }

  protected recargarFicha(): void {
    this.cargarFicha();
  }

  /**
   * Recibe cambios parciales desde las pestañas General/Estadísticas/
   * Combate, actualiza el estado local de inmediato (para que la UI
   * responda sin esperar al servidor) y programa el guardado
   * automático con debounce para no disparar una petición HTTP por
   * cada pulsación de tecla.
   */
  protected actualizarCampos(cambios: Partial<CamposEditables>): void {
    const actual = this.ficha();
    if (!actual || actual.soloLectura) return;

    const fichaActualizadaOptimista: PersonajeFicha = { ...actual, ...cambios };

    // Recalculamos los campos derivados localmente (modificadores de
    // atributo y bonificador de competencia) para que la UI no
    // muestre valores desactualizados mientras el autoguardado con
    // debounce espera antes de confirmar con el backend, que es la
    // única fuente de verdad definitiva tras la respuesta del PUT.
    if (
      'fuerza' in cambios ||
      'destreza' in cambios ||
      'constitucion' in cambios ||
      'inteligencia' in cambios ||
      'sabiduria' in cambios ||
      'carisma' in cambios
    ) {
      fichaActualizadaOptimista.modificadorFuerza = calcularModificador(fichaActualizadaOptimista.fuerza);
      fichaActualizadaOptimista.modificadorDestreza = calcularModificador(fichaActualizadaOptimista.destreza);
      fichaActualizadaOptimista.modificadorConstitucion = calcularModificador(fichaActualizadaOptimista.constitucion);
      fichaActualizadaOptimista.modificadorInteligencia = calcularModificador(fichaActualizadaOptimista.inteligencia);
      fichaActualizadaOptimista.modificadorSabiduria = calcularModificador(fichaActualizadaOptimista.sabiduria);
      fichaActualizadaOptimista.modificadorCarisma = calcularModificador(fichaActualizadaOptimista.carisma);

      // La percepción pasiva base depende del modificador de Sabiduría;
      // se recalcula sin bonificador de competencia aquí porque ese
      // dato (competencia en Percepción) no está disponible en este
      // nivel del componente. El valor exacto, con bonificador de
      // competencia si aplica, llega confirmado tras el guardado.
      fichaActualizadaOptimista.percepcionPasiva = 10 + fichaActualizadaOptimista.modificadorSabiduria;
    }

    if ('nivel' in cambios) {
      fichaActualizadaOptimista.bonificadorCompetencia = calcularBonificadorCompetencia(fichaActualizadaOptimista.nivel);
    }

    this.ficha.set(fichaActualizadaOptimista);

    this.pendienteDeGuardar = { ...this.pendienteDeGuardar, ...cambios };
    this.ultimoGuardadoOk.set(false);

    if (this.debounceTimer) {
      clearTimeout(this.debounceTimer);
    }

    this.debounceTimer = setTimeout(() => this.guardarInmediatamente(), DEBOUNCE_AUTOGUARDADO_MS);
  }

  private guardarInmediatamente(): void {
    const actual = this.ficha();
    if (!actual || !this.pendienteDeGuardar) return;

    this.debounceTimer = null;
    this.guardando.set(true);
    this.pendienteDeGuardar = null;

    const request: ActualizarPersonajeRequest = {
      nombre: actual.nombre,
      raza: actual.raza,
      clase: actual.clase,
      subclase: actual.subclase,
      trasfondo: actual.trasfondo,
      alineamiento: actual.alineamiento,
      imagenUrl: actual.imagenUrl,
      nivel: actual.nivel,
      experiencia: actual.experiencia,
      fuerza: actual.fuerza,
      destreza: actual.destreza,
      constitucion: actual.constitucion,
      inteligencia: actual.inteligencia,
      sabiduria: actual.sabiduria,
      carisma: actual.carisma,
      puntosVidaActuales: actual.puntosVidaActuales,
      puntosVidaMaximos: actual.puntosVidaMaximos,
      puntosVidaTemporales: actual.puntosVidaTemporales,
      dadoGolpe: actual.dadoGolpe,
      dadosGolpeRestantes: actual.dadosGolpeRestantes,
      claseArmadura: actual.claseArmadura,
      velocidad: actual.velocidad,
      iniciativaBonus: actual.iniciativaBonus,
      notasGenerales: actual.notasGenerales,
    };

    this.personajeService.actualizar(this.personajeId(), request).subscribe({
      next: (fichaActualizada) => {
        this.ficha.set(fichaActualizada);
        this.guardando.set(false);
        this.ultimoGuardadoOk.set(true);
      },
      error: () => {
        this.guardando.set(false);
      },
    });
  }
}
