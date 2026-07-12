export interface RepuestoDto {
  repuestoId:     number;
  facturaId:      number;
  incluido:       boolean;
  nombreRepuesto: string;
  costo:          number;
  fecha:          string;
  repuestera:     string;
  factura:        string | null;
}

export interface AgregarRepuestoRequest {
  facturaId:      number;
  nombreRepuesto: string;
  costo:          number;
  fecha:          string;
  repuestera:     string;
  numeroFactura?: string;
}

export interface ActualizarRepuestoRequest {
  nombreRepuesto: string;
  costo:          number;
  fecha:          string;
  repuestera:     string;
  numeroFactura?: string;
}
