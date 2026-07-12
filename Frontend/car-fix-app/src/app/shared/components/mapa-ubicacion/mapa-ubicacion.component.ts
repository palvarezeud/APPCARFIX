import {
  Component, AfterViewInit, OnDestroy, ElementRef,
  input, output, viewChild, effect
} from '@angular/core';
import * as L from 'leaflet';

L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'leaflet/marker-icon-2x.png',
  iconUrl:       'leaflet/marker-icon.png',
  shadowUrl:     'leaflet/marker-shadow.png',
});

const CENTRO_DEFECTO: L.LatLngTuple = [9.9281, -84.0907]; // San Jose, Costa Rica

@Component({
  selector: 'app-mapa-ubicacion',
  standalone: true,
  template: `<div #contenedorMapa class="mapa-ubicacion"></div>`,
  styles: [`
    .mapa-ubicacion {
      width: 100%; height: 320px;
      border-radius: var(--radio-borde);
      border: 1px solid var(--color-borde);
    }
  `]
})
export class MapaUbicacionComponent implements AfterViewInit, OnDestroy {
  latitud     = input<number | null>(null);
  longitud    = input<number | null>(null);
  soloLectura = input(false);

  ubicacionCambiada = output<{ latitud: number; longitud: number }>();

  private contenedorRef = viewChild.required<ElementRef<HTMLDivElement>>('contenedorMapa');
  private mapa?: L.Map;
  private marcador?: L.Marker;

  constructor() {
    effect(() => {
      const lat = this.latitud();
      const lon = this.longitud();
      if (!this.mapa || !this.marcador || lat == null || lon == null) return;

      const actual = this.marcador.getLatLng();
      if (Math.abs(actual.lat - lat) > 1e-9 || Math.abs(actual.lng - lon) > 1e-9) {
        this.marcador.setLatLng([lat, lon]);
        this.mapa.setView([lat, lon], this.mapa.getZoom());
      }
    });
  }

  ngAfterViewInit(): void {
    const hayCoordenadas = this.latitud() != null && this.longitud() != null;
    const centro: L.LatLngTuple = hayCoordenadas ? [this.latitud()!, this.longitud()!] : CENTRO_DEFECTO;

    this.mapa = L.map(this.contenedorRef().nativeElement).setView(centro, hayCoordenadas ? 15 : 12);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
      maxZoom: 19
    }).addTo(this.mapa);

    this.marcador = L.marker(centro, { draggable: !this.soloLectura() }).addTo(this.mapa);
    this.marcador.on('dragend', () => this.emitirPosicion());

    this.mapa.on('click', (e: L.LeafletMouseEvent) => {
      if (this.soloLectura()) return;
      this.marcador!.setLatLng(e.latlng);
      this.emitirPosicion();
    });
  }

  private emitirPosicion(): void {
    const pos = this.marcador!.getLatLng();
    this.ubicacionCambiada.emit({ latitud: pos.lat, longitud: pos.lng });
  }

  ngOnDestroy(): void {
    this.mapa?.remove();
  }
}
