import { ChangeDetectionStrategy, Component, computed, inject, input, output, signal } from '@angular/core';
import { InventarioItem } from '../../../../core/models';
import { PersonajeService } from '../../../../core/services/personaje.service';
import { ConfirmDialogService } from '../../../../shared/components/confirm-dialog/confirm-dialog.service';

interface NuevoItemForm {
  nombre: string;
  descripcion: string;
  cantidad: number;
  peso: number | null;
  tipo: string;
  equipado: boolean;
}

function nuevoItemVacio(): NuevoItemForm {
  return { nombre: '', descripcion: '', cantidad: 1, peso: null, tipo: '', equipado: false };
}

@Component({
  selector: 'app-inventory-tab',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="inventory-tab">
      @if (items().length === 0) {
        <p class="empty-state">El inventario está vacío.</p>
      } @else {
        <table class="inventory-tab__tabla">
          <thead>
            <tr>
              <th>Nombre</th>
              <th>Tipo</th>
              <th>Cantidad</th>
              <th>Peso</th>
              <th>Equipado</th>
              @if (!soloLectura()) {
                <th></th>
              }
            </tr>
          </thead>
          <tbody>
            @for (item of items(); track item.id) {
              <tr>
                <td>
                  <strong>{{ item.nombre }}</strong>
                  @if (item.descripcion) {
                    <p class="inventory-tab__descripcion">{{ item.descripcion }}</p>
                  }
                </td>
                <td>{{ item.tipo || '—' }}</td>
                <td class="stat-figure">{{ item.cantidad }}</td>
                <td class="stat-figure">{{ item.peso ?? '—' }}</td>
                <td>
                  @if (item.equipado) {
                    <span class="badge badge--success">Sí</span>
                  } @else {
                    <span class="badge badge--muted">No</span>
                  }
                </td>
                @if (!soloLectura()) {
                  <td>
                    <button type="button" class="btn btn--danger btn--sm" (click)="eliminarItem(item)">Quitar</button>
                  </td>
                }
              </tr>
            }
          </tbody>
        </table>
      }

      @if (!soloLectura()) {
        <div class="inventory-tab__nuevo">
          <h3>Añadir ítem</h3>
          <div class="inventory-tab__nuevo-fila">
            <input class="input" type="text" placeholder="Nombre" [value]="nuevoItem().nombre" (input)="set('nombre', $any($event.target).value)" />
            <input class="input" type="text" placeholder="Tipo (ej. Arma)" [value]="nuevoItem().tipo" (input)="set('tipo', $any($event.target).value)" />
            <input
              class="input"
              type="number"
              min="1"
              placeholder="Cant."
              [value]="nuevoItem().cantidad"
              (input)="set('cantidad', Number($any($event.target).value) || 1)"
            />
            <input
              class="input"
              type="number"
              step="0.1"
              placeholder="Peso"
              (input)="set('peso', $any($event.target).value ? Number($any($event.target).value) : null)"
            />
            <label class="inventory-tab__checkbox">
              <input type="checkbox" [checked]="nuevoItem().equipado" (change)="set('equipado', $any($event.target).checked)" />
              Equipado
            </label>
          </div>
          <input
            class="input"
            type="text"
            placeholder="Descripción (opcional)"
            [value]="nuevoItem().descripcion"
            (input)="set('descripcion', $any($event.target).value)"
          />
          <button type="button" class="btn btn--primary" [disabled]="!puedeAgregar() || guardando()" (click)="agregarItem()">
            {{ guardando() ? 'Añadiendo...' : '+ Añadir' }}
          </button>
        </div>
      }
    </div>
  `,
  styleUrl: './inventory-tab.component.scss',
})
export class InventoryTabComponent {
  readonly personajeId = input.required<number>();
  readonly items = input.required<InventarioItem[]>();
  readonly soloLectura = input.required<boolean>();
  readonly cambio = output<void>();

  private readonly personajeService = inject(PersonajeService);
  private readonly confirmDialog = inject(ConfirmDialogService);

  protected readonly nuevoItem = signal<NuevoItemForm>(nuevoItemVacio());
  protected readonly guardando = signal(false);

  protected readonly Number = Number;

  protected readonly puedeAgregar = computed(() => this.nuevoItem().nombre.trim().length > 0);

  protected set<K extends keyof NuevoItemForm>(campo: K, valor: NuevoItemForm[K]): void {
    this.nuevoItem.update((actual) => ({ ...actual, [campo]: valor }));
  }

  protected agregarItem(): void {
    if (!this.puedeAgregar()) return;

    this.guardando.set(true);
    const valores = this.nuevoItem();

    this.personajeService
      .agregarItemInventario(this.personajeId(), {
        nombre: valores.nombre,
        descripcion: valores.descripcion || null,
        cantidad: valores.cantidad,
        peso: valores.peso,
        equipado: valores.equipado,
        tipo: valores.tipo || null,
      })
      .subscribe({
        next: () => {
          this.guardando.set(false);
          this.nuevoItem.set(nuevoItemVacio());
          this.cambio.emit();
        },
        error: () => this.guardando.set(false),
      });
  }

  protected async eliminarItem(item: InventarioItem): Promise<void> {
    const confirmado = await this.confirmDialog.confirmar({
      titulo: 'Quitar ítem',
      mensaje: `¿Quitar "${item.nombre}" del inventario?`,
      textoConfirmar: 'Quitar',
      peligroso: true,
    });

    if (!confirmado) return;

    this.personajeService.eliminarItemInventario(this.personajeId(), item.id).subscribe({
      next: () => this.cambio.emit(),
    });
  }
}
