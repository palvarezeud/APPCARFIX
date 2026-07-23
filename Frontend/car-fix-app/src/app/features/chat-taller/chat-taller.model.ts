export type PasoChat = 'cliente' | 'vehiculo' | 'orden' | 'factura' | 'reparacion' | 'repuesto';
export type FaseChat = 'inicio' | 'confirmando' | 'ejecutando' | 'terminado';

export interface MensajeChat {
  id:      string;
  autor:   'usuario' | 'asistente';
  texto:   string;
  esError: boolean;
  fecha:   Date;
}
