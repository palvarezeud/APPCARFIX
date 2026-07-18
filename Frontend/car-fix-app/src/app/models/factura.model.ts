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
  descuento:                number;
  subTotal:                 number;
  impuestoVentas:           number;
  total:                    number;
  adelanto:                 number;
  pendiente:                number;
  estadoFacturaId:          number;
  estadoFacturaDescripcion: string;
}

export interface CrearFacturaRequest {
  vehiculoId:         number;
  fecha:              string;
  descripcionGeneral: string;
  descuento:          number;
  adelanto:           number;
}

export interface ActualizarFacturaRequest {
  fecha:              string;
  descripcionGeneral: string;
  descuento:          number;
  adelanto:           number;
}

export const ESTADOS_FACTURA: Record<number, string> = {
  1: 'Cotizacion',
  2: 'Pendiente',
  3: 'Pagada'
};
