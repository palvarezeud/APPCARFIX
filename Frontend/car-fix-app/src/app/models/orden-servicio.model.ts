export interface OrdenServicioDto {
  ordenServicioId:            number;
  vehiculoId:                 number;
  facturaId:                  number;
  placa:                      string;
  marca:                      string;
  modelo:                     string;
  nombreCliente:              string;
  fechaIngreso:               string;
  fechaSalida:                string;
  problemaGeneral:            string;
  estadoOrdenId:              number;
  estadoOrdenDescripcion:     string;
  esGarantia:                 boolean;
  facturaFecha:               string;
  facturaTotalRepuestos:      number;
  facturaTotalReparaciones:   number;
  facturaDescuento:           number;
  facturaAdelanto:            number;
  facturaImpuestoVentas:      number;
  facturaTotal:               number;
  facturaEstadoId:            number;
  facturaEstadoDescripcion:   string;
  facturaDescripcionGeneral:  string | null;
}

export interface CrearOrdenResponseDto {
  ordenServicioId: number;
  facturaId:       number;
}

export const ESTADOS_ORDEN: Record<number, string> = {
  1: 'Cotización',
  2: 'Recibido',
  3: 'Reparando',
  4: 'Finalizado',
  5: 'Entregado'
};
