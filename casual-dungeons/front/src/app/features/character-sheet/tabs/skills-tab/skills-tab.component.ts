import { ChangeDetectionStrategy, Component, inject, input, output } from '@angular/core';
import { PersonajeHabilidad } from '../../../../core/models';
import { PersonajeService } from '../../../../core/services/personaje.service';

@Component({
  selector: 'app-skills-tab',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <table class="skills-tab__tabla">
      <thead>
        <tr>
          <th>Habilidad</th>
          <th>Atributo</th>
          <th>Competente</th>
          <th>Experticia</th>
        </tr>
      </thead>
      <tbody>
        @for (habilidad of habilidades(); track habilidad.id) {
          <tr>
            <td>{{ habilidad.nombreHabilidad }}</td>
            <td class="skills-tab__atributo">{{ habilidad.atributoBase }}</td>
            <td>
              <input
                type="checkbox"
                [checked]="habilidad.competente"
                [disabled]="soloLectura()"
                (change)="actualizar(habilidad, $any($event.target).checked, habilidad.experticia)"
              />
            </td>
            <td>
              <input
                type="checkbox"
                [checked]="habilidad.experticia"
                [disabled]="soloLectura() || !habilidad.competente"
                (change)="actualizar(habilidad, habilidad.competente, $any($event.target).checked)"
              />
            </td>
          </tr>
        }
      </tbody>
    </table>
  `,
  styleUrl: './skills-tab.component.scss',
})
export class SkillsTabComponent {
  readonly personajeId = input.required<number>();
  readonly habilidades = input.required<PersonajeHabilidad[]>();
  readonly soloLectura = input.required<boolean>();
  readonly cambio = output<void>();

  private readonly personajeService = inject(PersonajeService);

  protected actualizar(habilidad: PersonajeHabilidad, competente: boolean, experticia: boolean): void {
    if (this.soloLectura()) return;

    // La experticia requiere competencia; si se desmarca competencia,
    // se desmarca también experticia automáticamente.
    const experticiaFinal = competente ? experticia : false;

    this.personajeService.actualizarHabilidad(this.personajeId(), habilidad.id, competente, experticiaFinal).subscribe({
      next: () => this.cambio.emit(),
    });
  }
}
