import { Component, ElementRef, ViewChild, afterRenderEffect, inject } from '@angular/core';
import { ChatTallerService } from './chat-taller.service';

@Component({
  selector: 'app-chat-taller',
  standalone: true,
  providers: [ChatTallerService],
  template: `
    <div class="chat-contenedor">
      <div class="chat-encabezado">
        <h2>Asistente de voz del taller</h2>
        <p class="chat-subtitulo">Diga que cliente, vehiculo u orden desea crear.</p>
      </div>

      <div class="chat-mensajes" #listaMensajes>
        @if (chat.mensajes().length === 0) {
          <p class="chat-vacio">
            Toque el microfono y diga, por ejemplo:<br>
            "Crea un cliente Juan Perez, telefono 8888-8888"
          </p>
        }
        @for (m of chat.mensajes(); track m.id) {
          <div class="chat-burbuja" [class.chat-burbuja-usuario]="m.autor === 'usuario'"
               [class.chat-burbuja-error]="m.esError">
            {{ m.texto }}
          </div>
        }
        @if (chat.procesando()) {
          <div class="chat-burbuja chat-burbuja-procesando">Procesando...</div>
        }
      </div>

      <div class="chat-pie">
        <button
          class="chat-microfono"
          [class.chat-microfono-escuchando]="chat.escuchando()"
          [disabled]="!chat.soportado() || chat.procesando()"
          [title]="tituloBoton()"
          (click)="chat.escucharTurno()">
          &#127908;
        </button>
        <p class="chat-estado">{{ tituloBoton() }}</p>
      </div>
    </div>
  `,
  styles: [`
    .chat-contenedor {
      display: flex; flex-direction: column; height: 100%;
      min-height: 100dvh;
    }
    .chat-encabezado {
      padding: 16px; text-align: center; border-bottom: 1px solid var(--color-borde);
    }
    .chat-encabezado h2 { margin: 0; font-size: 18px; }
    .chat-subtitulo { margin: 4px 0 0; font-size: 13px; color: var(--color-texto-suave); }

    .chat-mensajes {
      flex: 1; overflow-y: auto; padding: 16px;
      display: flex; flex-direction: column; gap: 10px;
    }
    .chat-vacio {
      text-align: center; color: var(--color-texto-suave); margin-top: 40px; line-height: 1.6;
    }
    .chat-burbuja {
      max-width: 85%; padding: 10px 14px; border-radius: var(--radio-borde);
      background: var(--color-tarjeta); border: 1px solid var(--color-borde);
      align-self: flex-start; font-size: 14px; line-height: 1.4;
    }
    .chat-burbuja-usuario {
      align-self: flex-end; background: var(--color-primario); color: white; border: none;
    }
    .chat-burbuja-error { border-color: var(--color-peligro); color: var(--color-peligro); }
    .chat-burbuja-procesando { align-self: flex-start; font-style: italic; color: var(--color-texto-suave); }

    .chat-pie {
      padding: 20px 16px 28px; display: flex; flex-direction: column; align-items: center; gap: 10px;
      border-top: 1px solid var(--color-borde);
    }
    .chat-microfono {
      width: 84px; height: 84px; border-radius: 50%;
      background: var(--color-primario); color: white; border: none;
      font-size: 36px; cursor: pointer; box-shadow: 0 2px 14px rgba(0,0,0,.3);
      display: flex; align-items: center; justify-content: center;
      transition: background .15s, transform .15s;
    }
    .chat-microfono:disabled { background: #adb5bd; cursor: not-allowed; box-shadow: none; }
    .chat-microfono:not(:disabled):active { transform: scale(0.96); }
    .chat-microfono-escuchando { background: var(--color-peligro); animation: chat-pulso 1.2s infinite; }
    .chat-estado { margin: 0; font-size: 13px; color: var(--color-texto-suave); text-align: center; }

    @keyframes chat-pulso {
      0%, 100% { box-shadow: 0 0 0 0 rgba(233,69,96,.5); }
      50% { box-shadow: 0 0 0 16px rgba(233,69,96,0); }
    }
  `]
})
export class ChatTallerComponent {
  protected readonly chat = inject(ChatTallerService);

  @ViewChild('listaMensajes') private listaMensajes?: ElementRef<HTMLDivElement>;

  constructor() {
    afterRenderEffect(() => {
      this.chat.mensajes();
      const el = this.listaMensajes?.nativeElement;
      if (el) el.scrollTop = el.scrollHeight;
    });
  }

  protected tituloBoton(): string {
    if (!this.chat.soportado()) {
      return 'Su navegador no soporta reconocimiento de voz. Pruebe con Chrome en Android.';
    }
    if (this.chat.escuchando())  return 'Escuchando... hable ahora';
    if (this.chat.procesando())  return 'Procesando...';
    return 'Toque para hablar';
  }
}
