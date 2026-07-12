const LADO_MAXIMO_PX = 1600;
const CALIDAD_JPEG    = 0.8;

export function comprimirImagen(archivo: File): Promise<File> {
  return new Promise((resolve, reject) => {
    const lector = new FileReader();
    lector.onload = () => {
      const img = new Image();
      img.onload = () => {
        let { width, height } = img;
        if (width > LADO_MAXIMO_PX || height > LADO_MAXIMO_PX) {
          const escala = LADO_MAXIMO_PX / Math.max(width, height);
          width  = Math.round(width * escala);
          height = Math.round(height * escala);
        }
        const canvas = document.createElement('canvas');
        canvas.width = width;
        canvas.height = height;
        const ctx = canvas.getContext('2d');
        if (!ctx) { reject(new Error('No se pudo procesar la imagen.')); return; }
        ctx.drawImage(img, 0, 0, width, height);
        canvas.toBlob(blob => {
          if (!blob) { reject(new Error('No se pudo comprimir la imagen.')); return; }
          resolve(new File([blob], archivo.name.replace(/\.[^.]+$/, '.jpg'), { type: 'image/jpeg' }));
        }, 'image/jpeg', CALIDAD_JPEG);
      };
      img.onerror = () => reject(new Error('No se pudo leer la imagen.'));
      img.src = lector.result as string;
    };
    lector.onerror = () => reject(new Error('No se pudo leer el archivo.'));
    lector.readAsDataURL(archivo);
  });
}
