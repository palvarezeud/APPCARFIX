export interface TallerDto {
  tallerId:             number;
  nombre:               string;
  ubicacionDescripcion: string;
  telefonos:            string;
  email:                string;
  latitud:              number | null;
  longitud:             number | null;
}
