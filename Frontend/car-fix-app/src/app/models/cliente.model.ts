export interface ClienteDto {
  clienteId:     number;
  nombreCliente: string;
  telefono1:     string;
  telefono2:     string | null;
  email:         string | null;
  esEmpresa:     boolean;
}

export interface CrearClienteRequest {
  nombreCliente: string;
  telefono1:     string;
  telefono2:     string | null;
  email:         string | null;
  esEmpresa:     boolean;
}
