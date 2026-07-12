using System.Text.Json;
using CarFix.Dominio.Interfaces;

namespace CarFix.Infraestructura.IA;

public class ServicioAsistenteVozAnthropic : IServicioAsistenteVoz
{
    private readonly IServicioLlamadaAnthropicJson _llamadaAnthropic;

    private const string InstruccionSistema =
        "Eres un asistente de voz para mecanicos de un taller automotriz que usan la app " +
        "CAR_FIX desde su celular. El mecanico habla un comando y tu debes interpretarlo. " +
        "Pantallas validas: clientes, vehiculos, ordenes, facturas, usuarios, configuracion, " +
        "catalogo-reparaciones, catalogo-repuestos. " +
        "Si el comando es para ir a una pantalla, usa intent=navegar y llena pantallaDestino. " +
        "Si pide buscar algo (ej. un cliente por nombre), usa intent=buscar, pantallaDestino " +
        "de donde debe buscarse, y terminoBusqueda. " +
        "Si pide crear un cliente nuevo dictando sus datos, usa intent=crear_cliente y llena " +
        "el objeto cliente con los campos que menciono (deja null los que no menciono). " +
        "Si pide crear un vehiculo nuevo dictando sus datos, usa intent=crear_vehiculo y llena " +
        "el objeto vehiculo (nombreClienteBuscado si menciona el nombre del dueño). " +
        "Si pide crear una orden de servicio dictando el problema del vehiculo, usa " +
        "intent=crear_orden y llena el objeto orden (problemaGeneral, esGarantia si lo " +
        "menciona, y placaBuscada solo si el usuario se refiere a un vehiculo existente " +
        "por su placa en lugar de uno recien creado en la conversacion). " +
        "Si pide enviar la factura, cotizacion o comprobante por correo, usa " +
        "intent=enviar_factura (no necesita datos adicionales, siempre se refiere a la " +
        "ultima factura de la conversacion actual). " +
        "Si pide crear otro tipo de registro sin dictar datos especificos, usa " +
        "intent=navegar con abrirFormularioCrear=true hacia la pantalla correspondiente. " +
        "Si no entiendes el comando, usa intent=desconocido y explica brevemente en " +
        "mensajeParaUsuario. Nunca inventes datos que la persona no dijo. " +
        "Si 'Etapa en curso del flujo encadenado' no es 'ninguna', el usuario ya inicio una " +
        "creacion encadenada de cliente-vehiculo-orden-factura y esta completando o " +
        "corrigiendo datos de esa etapa. Prioriza interpretar la transcripcion como datos " +
        "para esa etapa (cliente, vehiculo u orden segun corresponda) en lugar de adivinar " +
        "un intent nuevo, incluso si la transcripcion es la misma frase larga original " +
        "repetida con una etapa distinta indicada. Si la transcripcion no aporta ningun " +
        "dato util para esa etapa, usa intent=desconocido.";

    private static readonly Dictionary<string, JsonElement> EsquemaJson =
        JsonSerializer.Deserialize<Dictionary<string, JsonElement>>("""
        {
          "type": "object",
          "properties": {
            "intent": { "type": "string", "enum": ["navegar", "buscar", "crear_cliente", "crear_vehiculo", "crear_orden", "enviar_factura", "desconocido"] },
            "pantallaDestino": { "type": ["string", "null"] },
            "abrirFormularioCrear": { "type": "boolean" },
            "terminoBusqueda": { "type": ["string", "null"] },
            "cliente": {
              "type": ["object", "null"],
              "properties": {
                "nombreCliente": { "type": ["string", "null"] },
                "telefono1":     { "type": ["string", "null"] },
                "telefono2":     { "type": ["string", "null"] },
                "email":         { "type": ["string", "null"] },
                "esEmpresa":     { "type": ["boolean", "null"] }
              },
              "required": ["nombreCliente", "telefono1", "telefono2", "email", "esEmpresa"],
              "additionalProperties": false
            },
            "vehiculo": {
              "type": ["object", "null"],
              "properties": {
                "placa":                { "type": ["string", "null"] },
                "marca":                { "type": ["string", "null"] },
                "modelo":               { "type": ["string", "null"] },
                "vin":                  { "type": ["string", "null"] },
                "annio":                { "type": ["integer", "null"] },
                "motor":                { "type": ["string", "null"] },
                "esAutomatico":         { "type": ["boolean", "null"] },
                "nombreClienteBuscado": { "type": ["string", "null"] }
              },
              "required": ["placa", "marca", "modelo", "vin", "annio", "motor", "esAutomatico", "nombreClienteBuscado"],
              "additionalProperties": false
            },
            "orden": {
              "type": ["object", "null"],
              "properties": {
                "problemaGeneral": { "type": ["string", "null"] },
                "esGarantia":      { "type": ["boolean", "null"] },
                "placaBuscada":    { "type": ["string", "null"] }
              },
              "required": ["problemaGeneral", "esGarantia", "placaBuscada"],
              "additionalProperties": false
            },
            "mensajeParaUsuario": { "type": ["string", "null"] }
          },
          "required": ["intent", "pantallaDestino", "abrirFormularioCrear", "terminoBusqueda", "cliente", "vehiculo", "orden", "mensajeParaUsuario"],
          "additionalProperties": false
        }
        """)!;

    public ServicioAsistenteVozAnthropic(IServicioLlamadaAnthropicJson llamadaAnthropic)
    {
        _llamadaAnthropic = llamadaAnthropic;
    }

    public async Task<InterpretacionVozResultado> InterpretarAsync(
        string transcripcion, string? pantallaActual, string? intentEnProgreso, CancellationToken ct = default)
    {
        var contenido = $"Pantalla actual: {pantallaActual ?? "desconocida"}\n" +
                        $"Etapa en curso del flujo encadenado: {intentEnProgreso ?? "ninguna"}\n" +
                        $"Comando: {transcripcion}";

        var resultado = await _llamadaAnthropic.LlamarAsync(
            InstruccionSistema, contenido, EsquemaJson, maxTokens: 512, ct);

        if (!resultado.EsExitoso)
            return InterpretacionVozResultado.Fallo(resultado.Error!);

        var raiz = resultado.Valor;

        var cliente = LeerObjeto(raiz, "cliente", c => new ClienteExtraidoVoz(
            LeerString(c, "nombreCliente"),
            LeerString(c, "telefono1"),
            LeerString(c, "telefono2"),
            LeerString(c, "email"),
            LeerBool(c, "esEmpresa")));

        var vehiculo = LeerObjeto(raiz, "vehiculo", v => new VehiculoExtraidoVoz(
            LeerString(v, "placa"),
            LeerString(v, "marca"),
            LeerString(v, "modelo"),
            LeerString(v, "vin"),
            LeerInt(v, "annio"),
            LeerString(v, "motor"),
            LeerBool(v, "esAutomatico"),
            LeerString(v, "nombreClienteBuscado")));

        var orden = LeerObjeto(raiz, "orden", o => new OrdenExtraidaVoz(
            LeerString(o, "problemaGeneral"),
            LeerBool(o, "esGarantia"),
            LeerString(o, "placaBuscada")));

        return InterpretacionVozResultado.Exito(
            LeerString(raiz, "intent") ?? "desconocido",
            NormalizarPantalla(LeerString(raiz, "pantallaDestino")),
            raiz.TryGetProperty("abrirFormularioCrear", out var af) && af.ValueKind == JsonValueKind.True,
            LeerString(raiz, "terminoBusqueda"),
            cliente,
            vehiculo,
            orden,
            LeerString(raiz, "mensajeParaUsuario"));
    }

    private static readonly HashSet<string> PantallasValidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "clientes", "vehiculos", "ordenes", "facturas", "usuarios",
        "configuracion", "catalogo-reparaciones", "catalogo-repuestos"
    };

    private static string? NormalizarPantalla(string? pantalla) =>
        pantalla is not null && PantallasValidas.Contains(pantalla) ? pantalla : null;

    private static T? LeerObjeto<T>(JsonElement raiz, string propiedad, Func<JsonElement, T> mapear) where T : class =>
        raiz.TryGetProperty(propiedad, out var valor) && valor.ValueKind == JsonValueKind.Object
            ? mapear(valor)
            : null;

    private static string? LeerString(JsonElement elemento, string propiedad) =>
        elemento.TryGetProperty(propiedad, out var valor) && valor.ValueKind == JsonValueKind.String
            ? valor.GetString()
            : null;

    private static int? LeerInt(JsonElement elemento, string propiedad) =>
        elemento.TryGetProperty(propiedad, out var valor) && valor.ValueKind == JsonValueKind.Number
            ? valor.GetInt32()
            : null;

    private static bool? LeerBool(JsonElement elemento, string propiedad) =>
        elemento.TryGetProperty(propiedad, out var valor) && valor.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? valor.GetBoolean()
            : null;
}
