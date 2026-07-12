using CarFix.Especificaciones.Soporte;
using Reqnroll;

namespace CarFix.Especificaciones.Steps;

[Binding]
public class PasosCompartidos
{
    private readonly ContextoEscenario _contextoEscenario;

    public PasosCompartidos(ContextoEscenario contextoEscenario)
        => _contextoEscenario = contextoEscenario;

    [Then(@"la validacion falla con ""(.*)""")]
    public void EntoncesValidacionFalla(string mensajeEsperado)
    {
        Assert.That(_contextoEscenario.ErrorValidacion, Is.Not.Null,
            "Se esperaba un error de validacion pero no ocurrio.");
        Assert.That(_contextoEscenario.ErrorValidacion, Does.Contain(mensajeEsperado));
    }
}
