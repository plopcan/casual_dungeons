import { Injectable, signal } from '@angular/core';

interface SolicitudConfirmacion {
  titulo: string;
  mensaje: string;
  textoConfirmar: string;
  textoCancelar: string;
  peligroso: boolean;
  resolver: (confirmado: boolean) => void;
}

/**
 * Gestiona un único diálogo de confirmación global, renderizado
 * una vez en AppComponent. Cualquier componente puede invocar
 * confirmar() y esperar la respuesta del usuario de forma asíncrona,
 * en lugar de usar window.confirm() (que no respeta el tema visual).
 */
@Injectable({ providedIn: 'root' })
export class ConfirmDialogService {
  private readonly _solicitud = signal<SolicitudConfirmacion | null>(null);

  readonly solicitud = this._solicitud.asReadonly();

  confirmar(opciones: {
    titulo: string;
    mensaje: string;
    textoConfirmar?: string;
    textoCancelar?: string;
    peligroso?: boolean;
  }): Promise<boolean> {
    return new Promise<boolean>((resolve) => {
      this._solicitud.set({
        titulo: opciones.titulo,
        mensaje: opciones.mensaje,
        textoConfirmar: opciones.textoConfirmar ?? 'Confirmar',
        textoCancelar: opciones.textoCancelar ?? 'Cancelar',
        peligroso: opciones.peligroso ?? false,
        resolver: resolve,
      });
    });
  }

  responder(confirmado: boolean): void {
    const actual = this._solicitud();
    if (!actual) return;

    actual.resolver(confirmado);
    this._solicitud.set(null);
  }
}
