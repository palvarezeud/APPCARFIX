export interface VehiculoDto {
  vehiculoId:         number;
  clienteId:          number;
  nombreCliente:      string;
  placa:              string | null;
  marca:              string;
  modelo:             string | null;
  vin:                string | null;
  annio:              number | null;
  motor:              string | null;
  esAutomatico:       boolean;
  detallesCarroceria: string;
}
