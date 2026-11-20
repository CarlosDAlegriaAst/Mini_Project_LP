namespace Parqueadero.Core.Services;

public static class ColeccionesExtensions
{
    /// <summary>Genérico + iterador (yield) + delegado Func.</summary>
    public static IEnumerable<T> Filtrar<T>(this IEnumerable<T> origen, Func<T, bool> condicion)
    {
        foreach (var item in origen)
            if (condicion(item)) yield return item;
    }

    public static List<T> Pagina<T>(this IEnumerable<T> origen, int pagina, int tamano) =>
        origen.Skip((Math.Max(1, pagina) - 1) * tamano).Take(tamano).ToList();
}
