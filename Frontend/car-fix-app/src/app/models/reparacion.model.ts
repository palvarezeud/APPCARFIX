export interface ReparacionDto {
  reparacionId:            number;
  facturaId:               number;
  listo:                   boolean;
  descripcionReparacion:   string;
  duracionAproximadaHoras: number | null;
  costo:                   number;
}

export interface AgregarReparacionRequest {
  facturaId:               number;
  descripcionReparacion:   string;
  costo:                   number;
  duracionAproximadaHoras?: number;
}

export interface ActualizarReparacionRequest {
  descripcionReparacion:    string;
  costo:                    number;
  duracionAproximadaHoras?: number | null;
}
