export interface UsuarioDto {
  usuarioId:      number;
  nombreUsuario:  string;
  nombreCompleto: string;
  email:          string | null;
  activo:         boolean;
  rolId:          number;
  nombreRol:      string;
}

export interface CrearUsuarioRequest {
  nombreUsuario:  string;
  password:       string;
  nombreCompleto: string;
  email:          string | null;
  rolId:          number;
  activo:         boolean;
}

export interface ActualizarUsuarioRequest {
  nombreCompleto: string;
  email:          string | null;
  rolId:          number;
  activo:         boolean;
}

export interface CambiarContrasennaRequest {
  nuevoPassword: string;
}

export const ROLES = [
  { rolId: 1, nombre: 'Administrador' },
  { rolId: 2, nombre: 'JefeMecanicos' },
  { rolId: 3, nombre: 'Mecanico' }
] as const;
