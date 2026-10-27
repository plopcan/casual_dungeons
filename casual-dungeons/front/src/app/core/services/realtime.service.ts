import { Injectable, inject, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';

/**
 * Gestiona la conexión SignalR al CampaignHub del backend. El
 * componente que muestra la vista de una campaña (DM o jugador)
 * debe llamar a unirseACampania() al inicializarse y
 * salirDeCampania() al destruirse.
 *
 * Los eventos recibidos se exponen como Signals que se actualizan
 * con el Id de la entidad afectada; los componentes interesados
 * pueden usar un `effect()` sobre estos signals para refrescar sus
 * datos vía HTTP sin necesidad de recargar la página.
 */
@Injectable({ providedIn: 'root' })
export class RealtimeService {
  private readonly authService = inject(AuthService);
  private connection: signalR.HubConnection | null = null;
  private campaniaActualId: number | null = null;

  /** Id del último personaje notificado como actualizado. */
  readonly personajeActualizado = signal<number | null>(null);

  /** Id del último NPC notificado como actualizado. */
  readonly npcActualizado = signal<number | null>(null);

  /** Id del último monstruo de campaña notificado como actualizado. */
  readonly monstruoCampaniaActualizado = signal<number | null>(null);

  /** Id del último combate notificado como actualizado. */
  readonly combateActualizado = signal<number | null>(null);

  /** Estado de la conexión, útil para mostrar un indicador visual. */
  readonly conectado = signal(false);

  private async asegurarConexion(): Promise<signalR.HubConnection> {
    if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
      return this.connection;
    }

    const token = this.authService.token();

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(environment.hubUrl, {
        accessTokenFactory: () => token ?? '',
      })
      .withAutomaticReconnect()
      .build();

    this.connection.on('PersonajeActualizado', (personajeId: number) => {
      this.personajeActualizado.set(personajeId);
    });

    this.connection.on('NPCActualizado', (npcId: number) => {
      this.npcActualizado.set(npcId);
    });

    this.connection.on('MonstruoCampaniaActualizado', (monstruoCampaniaId: number) => {
      this.monstruoCampaniaActualizado.set(monstruoCampaniaId);
    });

    this.connection.on('CombateActualizado', (combateId: number) => {
      this.combateActualizado.set(combateId);
    });

    this.connection.onreconnected(() => {
      this.conectado.set(true);
      if (this.campaniaActualId !== null) {
        this.connection?.invoke('UnirseACampania', this.campaniaActualId);
      }
    });

    this.connection.onreconnecting(() => this.conectado.set(false));
    this.connection.onclose(() => this.conectado.set(false));

    await this.connection.start();
    this.conectado.set(true);

    return this.connection;
  }

  async unirseACampania(campaniaId: number): Promise<void> {
    const connection = await this.asegurarConexion();
    this.campaniaActualId = campaniaId;
    await connection.invoke('UnirseACampania', campaniaId);
  }

  async salirDeCampania(campaniaId: number): Promise<void> {
    if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('SalirDeCampania', campaniaId);
    }
    this.campaniaActualId = null;
  }

  async desconectar(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
      this.conectado.set(false);
    }
  }
}
