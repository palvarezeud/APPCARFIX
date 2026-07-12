using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.Autenticacion.Commands.IniciarSesion;
using CarFix.Dominio.Entidades;
using CarFix.Dominio.Interfaces;
using CarFix.Infraestructura.Persistencia;
using MediatR;
using Reqnroll;

namespace CarFix.Especificaciones.Steps;

[Binding]
public class AutenticacionSteps
{
    private readonly ISender              _sender;
    private readonly CarFixDbContext      _contexto;
    private readonly IServicioContrasenna _servicioContrasenna;

    private Resultado<RespuestaTokenDto>? _ultimoResultado;

    public AutenticacionSteps(
        ISender              sender,
        CarFixDbContext      contexto,
        IServicioContrasenna servicioContrasenna)
    {
        _sender              = sender;
        _contexto            = contexto;
        _servicioContrasenna = servicioContrasenna;
    }

    [Given(@"que existe un rol ""(.*)"" con ID (\d+)")]
    public void DadoExisteRol(string nombre, int rolId)
    {
        if (!_contexto.Roles.Any(r => r.RolId == rolId))
        {
            _contexto.Roles.Add(new Role { RolId = rolId, Nombre = nombre });
            _contexto.SaveChanges();
        }
    }

    [Given(@"que existe un usuario ""(.*)"" con contrasenna ""(.*)"" y rol ""(.*)""")]
    public void DadoExisteUsuario(string nombreUsuario, string contrasenna, string nombreRol)
    {
        var rol = _contexto.Roles.First(r => r.Nombre == nombreRol);
        _contexto.Usuarios.Add(new Usuario
        {
            NombreUsuario  = nombreUsuario,
            PasswordHash   = _servicioContrasenna.Hashear(contrasenna),
            NombreCompleto = "Usuario de prueba",
            Activo         = true,
            RolId          = rol.RolId
        });
        _contexto.SaveChanges();
    }

    [Given(@"que el usuario ""(.*)"" esta desactivado")]
    public void DadoUsuarioDesactivado(string nombreUsuario)
    {
        var usuario = _contexto.Usuarios.First(u => u.NombreUsuario == nombreUsuario);
        usuario.Activo = false;
        _contexto.SaveChanges();
    }

    [When(@"el usuario inicia sesion con ""(.*)"" y ""(.*)""")]
    public async Task CuandoIniciaSesion(string nombreUsuario, string password)
    {
        _ultimoResultado = await _sender.Send(new IniciarSesionCommand(nombreUsuario, password));
    }

    [Then(@"recibe un token JWT valido")]
    public void EntoncesTokenValido()
    {
        Assert.That(_ultimoResultado!.EsExitoso, Is.True,
            $"Se esperaba exito pero fallo con: {_ultimoResultado.Error}");
        Assert.That(_ultimoResultado.Valor!.Token, Is.Not.Empty);
    }

    [Then(@"el sistema rechaza el acceso con ""(.*)""")]
    public void EntoncesRechazo(string mensajeEsperado)
    {
        Assert.That(_ultimoResultado!.EsExitoso, Is.False,
            "Se esperaba fallo pero la operacion fue exitosa.");
        Assert.That(_ultimoResultado.Error, Is.EqualTo(mensajeEsperado));
    }
}
