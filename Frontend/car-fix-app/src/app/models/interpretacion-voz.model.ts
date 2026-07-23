export interface ClienteVozDto {
  nombreCliente: string  | null;
  telefono1:     string  | null;
  telefono2:     string  | null;
  email:         string  | null;
  esEmpresa:     boolean | null;
}

export interface VehiculoVozDto {
  placa:                string  | null;
  marca:                string  | null;
  modelo:               string  | null;
  vin:                  string  | null;
  annio:                number  | null;
  motor:                string  | null;
  esAutomatico:         boolean | null;
  nombreClienteBuscado: string  | null;
}

export interface OrdenVozDto {
  problemaGeneral: string  | null;
  esGarantia:      boolean | null;
  placaBuscada:    string  | null;
}

export interface FacturaVozDto {
  nombreClienteBuscado: string | null;
  placaBuscada:         string | null;
}

export interface ReparacionVozDto {
  descripcionReparacion:   string  | null;
  costo:                   number  | null;
  duracionAproximadaHoras: number  | null;
}

export interface RepuestoVozDto {
  nombreRepuesto: string | null;
  costo:          number | null;
  repuestera:     string | null;
  numeroFactura:  string | null;
}

export interface InterpretacionVozDto {
  intent:               string;
  pantallaDestino:      string | null;
  abrirFormularioCrear: boolean;
  terminoBusqueda:      string | null;
  cliente:              ClienteVozDto    | null;
  vehiculo:             VehiculoVozDto   | null;
  orden:                OrdenVozDto      | null;
  factura:              FacturaVozDto    | null;
  reparacion:           ReparacionVozDto | null;
  repuesto:             RepuestoVozDto   | null;
  mensajeParaUsuario:   string | null;
}
