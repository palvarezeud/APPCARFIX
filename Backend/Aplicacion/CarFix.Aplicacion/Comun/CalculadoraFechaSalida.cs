namespace CarFix.Aplicacion.Comun;

// El taller no labora sabados ni domingos: al sumar horas de reparacion a la
// fecha de ingreso, esos dias se saltan por completo.
public static class CalculadoraFechaSalida
{
    public static DateTime Calcular(DateTime fechaIngreso, int totalHorasReparacion)
    {
        var fecha = SaltarFinDeSemana(fechaIngreso);

        for (var i = 0; i < totalHorasReparacion; i++)
            fecha = SaltarFinDeSemana(fecha.AddHours(1));

        return fecha;
    }

    private static DateTime SaltarFinDeSemana(DateTime fecha)
    {
        while (fecha.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            fecha = fecha.AddDays(1);
        return fecha;
    }
}
