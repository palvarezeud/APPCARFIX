namespace CarFix.Aplicacion.Comun;

// Simula el avance del reloj dentro del horario laboral real del taller
// (horaApertura-horaCierre), saltando por completo sabados y domingos.
public static class CalculadoraFechaSalida
{
    public static DateTime Calcular(DateTime fechaIngreso, double totalHorasReparacion, TimeSpan horaApertura, TimeSpan horaCierre)
    {
        var fecha          = SiguienteMomentoLaboral(fechaIngreso, horaApertura, horaCierre);
        var horasRestantes = totalHorasReparacion;

        while (horasRestantes > 0)
        {
            var horasDisponiblesHoy = (horaCierre - fecha.TimeOfDay).TotalHours;

            if (horasRestantes <= horasDisponiblesHoy)
            {
                fecha = fecha.AddHours(horasRestantes);
                horasRestantes = 0;
            }
            else
            {
                horasRestantes -= horasDisponiblesHoy;
                fecha = SiguienteDiaHabil(fecha.Date.AddDays(1)).Add(horaApertura);
            }
        }

        return fecha;
    }

    private static DateTime SiguienteMomentoLaboral(DateTime fecha, TimeSpan apertura, TimeSpan cierre)
    {
        var soloFecha = fecha.Date;

        if (EsFinDeSemana(soloFecha))
            return SiguienteDiaHabil(soloFecha).Add(apertura);
        if (fecha.TimeOfDay < apertura)
            return soloFecha.Add(apertura);
        if (fecha.TimeOfDay >= cierre)
            return SiguienteDiaHabil(soloFecha.AddDays(1)).Add(apertura);

        return fecha;
    }

    private static DateTime SiguienteDiaHabil(DateTime fecha)
    {
        while (EsFinDeSemana(fecha))
            fecha = fecha.AddDays(1);
        return fecha;
    }

    private static bool EsFinDeSemana(DateTime fecha) =>
        fecha.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
}
