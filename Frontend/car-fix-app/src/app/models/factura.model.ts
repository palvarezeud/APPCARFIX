export interface FacturaDto {
  facturaId:                number;
  vehiculoId:               number;
  placa:                    string;
  marca:                    string;
  modelo:                   string;
  fecha:                    string;
  nombreCliente:            string;
  emailCliente:             string | null;
  descripcionGeneral:       string | null;
  totalRepuestos:           number;
  totalReparaciones:        number;
  total:                    number;
  descuento:                number;
  adelanto:                 number;
  impuestoVentas:           number;
  estadoFacturaId:          number;
  estadoFacturaDescripcion: string;
}

export interface CrearFacturaRequest {
  vehiculoId:         number;
  fecha:              string;
  descripcionGeneral: string;
  descuento:          number;
  adelanto:           number;
  impuestoVentas:     number;
}

export interface ActualizarFacturaRequest {
  fecha:              string;
  descripcionGeneral: string;
  descuento:          number;
  adelanto:           number;
  impuestoVentas:     number;
}

export const ESTADOS_FACTURA: Record<number, string> = {
  1: 'Cotizacion',
  2: 'Pendiente',
  3: 'Pagada'
};
