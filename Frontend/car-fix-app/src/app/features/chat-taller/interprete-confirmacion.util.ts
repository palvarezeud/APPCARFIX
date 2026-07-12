const PALABRAS_AFIRMATIVAS = [
  'si', 'sí', 'confirmar', 'confirmo', 'confirma', 'dale', 'correcto',
  'exacto', 'afirmativo', 'ok', 'okay', 'listo', 'de acuerdo', 'asi es', 'así es'
];

const PALABRAS_NEGATIVAS = [
  'no', 'cancelar', 'cancela', 'negativo', 'incorrecto', 'para', 'detente'
];

function normalizar(texto: string): string {
  return texto
    .toLowerCase()
    .normalize('NFD')
    .replace(new RegExp('[\\u0300-\\u036f]', 'g'), '')
    .trim();
}

/**
 * Clasifica la respuesta del usuario durante la fase de confirmacion del chat.
 * Solo debe invocarse cuando el chat esta esperando un si/no — fuera de ese
 * contexto, frases que empiezan con "no" (ej. "no tiene VIN") son datos, no negaciones.
 */
export function clasificarRespuesta(texto: string): 'afirmativo' | 'negativo' | 'correccion' {
  const normalizado = normalizar(texto);
  const primeraPalabra = normalizado.split(/\s+/)[0] ?? '';

  const esSoloAfirmacion = PALABRAS_AFIRMATIVAS.some(p => normalizado === p) ||
    (PALABRAS_AFIRMATIVAS.includes(primeraPalabra) && normalizado.split(/\s+/).length <= 2);

  if (esSoloAfirmacion) return 'afirmativo';

  const esSoloNegacion = PALABRAS_NEGATIVAS.some(p => normalizado === p) ||
    (['no', 'cancelar', 'cancela'].includes(primeraPalabra) && normalizado.split(/\s+/).length <= 2);

  if (esSoloNegacion) return 'negativo';

  // Frase larga (ej. "no, el telefono es 8888-7777") o corrección explícita:
  // se trata como corrección de datos, no como negativo puro.
  return 'correccion';
}
