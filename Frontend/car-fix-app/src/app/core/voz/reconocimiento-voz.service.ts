import { Injectable, signal } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ReconocimientoVozService {
  private readonly ctorReconocimiento =
    window.SpeechRecognition ?? window.webkitSpeechRecognition;

  private readonly _soportado = signal(!!this.ctorReconocimiento);
  readonly soportado = this._soportado.asReadonly();

  private reconocimientoActivo: SpeechRecognition | null = null;

  escuchar(): Observable<string> {
    return new Observable<string>(observer => {
      const Ctor = this.ctorReconocimiento;
      if (!Ctor) {
        observer.error(new Error('El reconocimiento de voz no esta soportado en este navegador.'));
        return;
      }

      const reconocimiento = new Ctor();
      reconocimiento.lang = 'es-CR';
      reconocimiento.continuous = false;
      reconocimiento.interimResults = false;
      reconocimiento.maxAlternatives = 1;

      let seRecibioResultado = false;

      reconocimiento.onresult = (ev: SpeechRecognitionEvent) => {
        seRecibioResultado = true;
        let transcript = '';
        try {
          transcript = ev.results.item(0)?.item(0)?.transcript
            ?? (ev.results as unknown as SpeechRecognitionResult[])[0]?.[0]?.transcript
            ?? '';
        } catch {
          transcript = '';
        }
        observer.next(transcript.trim());
        observer.complete();
      };

      reconocimiento.onerror = (ev: SpeechRecognitionErrorEvent) => {
        seRecibioResultado = true;
        observer.error(new Error(this.mensajeError(ev.error)));
      };

      reconocimiento.onend = () => {
        // Algunos navegadores (ej. Samsung Internet) terminan el reconocimiento
        // sin disparar onresult ni onerror cuando no soportan realmente el
        // reconocimiento de voz en la nube, aunque expongan el objeto global.
        if (!seRecibioResultado) {
          observer.next('');
        }
        observer.complete();
      };

      this.reconocimientoActivo = reconocimiento;
      reconocimiento.start();

      return () => {
        reconocimiento.onresult = null;
        reconocimiento.onerror = null;
        reconocimiento.onend = null;
        reconocimiento.abort();
        this.reconocimientoActivo = null;
      };
    });
  }

  detener(): void {
    this.reconocimientoActivo?.stop();
  }

  private mensajeError(codigo: string): string {
    switch (codigo) {
      case 'not-allowed':
      case 'service-not-allowed':
        return 'Se denego el permiso del microfono. Revise los permisos del sitio en su navegador.';
      case 'no-speech':
        return 'No se detecto ningun audio. Intente de nuevo.';
      case 'network':
        return 'Error de red al procesar el audio. Intente de nuevo.';
      default:
        return 'No se pudo reconocer el audio. Intente de nuevo.';
    }
  }
}
