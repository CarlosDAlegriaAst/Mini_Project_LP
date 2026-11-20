# Justificación de estructuras de datos

| Necesidad | Estructura elegida | Alternativa descartada | Razón |
|---|---|---|---|
| Ocupación por tipo de vehículo (`OcupacionAsync`) | `Dictionary<TipoVehiculo,(int,int)>` | `List<(Tipo,int,int)>` | Búsqueda por clave O(1) frente a O(n) recorriendo la lista; la clave es única por diseño. |
| Últimos movimientos | `Stack<string>` | `List<string>` con `Insert(0, …)` | LIFO: el más reciente sale primero con `Push` O(1); `Insert(0)` desplaza todos los elementos, O(n). |
| Lista de espera sin cupo | `Queue<(string,TipoVehiculo)>` | `List<…>` con `RemoveAt(0)` | FIFO justo: `Enqueue/Dequeue` en O(1); la lista copia elementos al quitar el primero. |
| Espacios y registros (resultados de BD) | `List<T>` | Arreglo `T[]` | Tamaño variable tras consultas LINQ; el arreglo exige conocer el tamaño de antemano. |
| Resumen de recaudo | `record ResumenTipo` + LINQ `GroupBy` | Clase mutable | Inmutabilidad y comparación por valor; menos código. |
| Resultado de operaciones | `record ResultadoOperacion` | Excepciones | Rechazos esperados (placa repetida, sin cupo) no son errores excepcionales; lanzar excepciones es más lento y ensucia el flujo. |
| Tipo de vehículo | `enum` | `string` | Valores cerrados, sin errores de digitación, comparación rápida. |

## Ejemplo comparativo: buscar ocupación del tipo Moto
- `List`: recorre hasta encontrar el elemento, **O(n)**.
- `Dictionary`: `ocupacion[TipoVehiculo.Moto]`, **O(1)** promedio.
Con 3 tipos la diferencia es mínima, pero el diseño escala si se agregan zonas, pisos o sedes.
