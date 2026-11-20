# Sistema de Gestión de Parqueadero

**Asignatura:** Lenguajes de Programación · **Miniproyecto:** 2 · **Fecha:** 17 de noviembre de 2026
**Universidad Santiago de Cali (USC)**

## Integrantes

| Nombre | Identificación |
|---|---|
| Álvaro Jose Jaramillo Gonzales | 1114541229 |
| Juan Camilo Henao Vargas | 1114309721 |
| Carlos David Alegría Astaíza | 1058547673 |

## Problemática
Un parqueadero pequeño suele llevar el control en papel: se pierden tiquetes, se cobra mal el tiempo y no se sabe cuántos cupos quedan. Esta solución registra entradas y salidas, asigna espacios, calcula el cobro y muestra la ocupación en tiempo real.

## Arquitectura compartida

```
src/
├── Parqueadero.Core      Biblioteca de clases: dominio + lógica de negocio (sin dependencias de UI ni BD)
├── Parqueadero.Data      Entity Framework Core (SQLite) + ASP.NET Core Identity + repositorio
├── Parqueadero.Web       ASP.NET Core MVC (Identity, roles, Bootstrap responsive)
└── Parqueadero.Desktop   Aplicación de escritorio Windows Forms
docs/EstructurasDeDatos.md  Justificación de estructuras de datos
```

Web y escritorio usan la **misma lógica (`ParqueaderoService`) y la misma base de datos** (`%LOCALAPPDATA%/Parqueadero/parqueadero.db`).

## Cumplimiento de requerimientos

| Requerimiento | Dónde |
|---|---|
| Biblioteca de clase base | `Parqueadero.Core` |
| Patrón MVC | `Parqueadero.Web` (Controllers / Views / modelos) |
| Aplicación de escritorio | `Parqueadero.Desktop` |
| Entity Framework Core | `AppDbContext`, `EfParqueaderoRepository` |
| Estructuras de datos justificadas | `docs/EstructurasDeDatos.md` |
| ASP.NET Core Identity y 2 roles | `Admin` y `Operador` (`DbSeeder`) |
| Diseño responsive | Bootstrap 5, rejilla fluida, tablas con scroll, menú colapsable |
| Repositorio GitHub | ver sección siguiente |

**Características de C# / .NET usadas (y dónde):**

| Característica | Ubicación |
|---|---|
| Clases abstractas y herencia | `EntidadBase`, `EstrategiaCobro` |
| Polimorfismo | `CobroHora` / `CobroMediaHora` (`EstrategiaCobro.Para`) |
| Interfaces e inyección de dependencias | `IParqueaderoRepository`, `Program.cs` |
| Genéricos, iteradores (`yield`) y delegados `Func<>` | `ColeccionesExtensions.Filtrar<T>` / `Pagina<T>` |
| Eventos y delegados | `VehiculoIngresado`, `VehiculoSalio` |
| Excepciones propias y `try/catch` | `ParqueaderoException`, `PlacaInvalidaException` |
| Expresiones regulares | `ValidadorPlaca` |
| Records, enums, tuplas, switch expressions, pattern matching | `Entidades.cs`, `ValidadorPlaca.cs` |
| Colecciones (`Dictionary`, `Stack`, `Queue`, `List`) | `ParqueaderoService` |
| LINQ | `ResumenAsync`, `OcupacionAsync` |
| `async/await`, `lock`, nullable reference types | servicio y repositorio |
| Métodos de extensión | `Normalizar`, `Filtrar`, `Pagina` |
| Data Annotations (validación) | `Models/ViewModels.cs` |
| Pruebas unitarias (xUnit) | `tests/Parqueadero.Tests` |

## Validaciones
- **Placa por tipo de vehículo:** carro `ABC123`, moto `ABC12D`, bicicleta de 3 a 8 letras o números. Se valida en el dominio, así que aplica igual en web y escritorio.
- **Web:** atributos HTML (`required`, `pattern`, `maxlength`) en el cliente y Data Annotations (`[Required]`, `[RegularExpression]`, `[Range]`) en el servidor, además de token antifalsificación.
- Placa repetida dentro del parqueadero, tarifa entre 0 y 1.000.000, y control de acceso por rol.

## Lista de espera
Si no hay cupo, el vehículo entra a una cola (`Queue`). Cuando sale uno del mismo tipo, el primero de la cola ingresa automáticamente.

## Pruebas y CI
```bash
dotnet test tests/Parqueadero.Tests
```
GitHub Actions (`.github/workflows/ci.yml`) compila la solución y ejecuta las pruebas en cada push.

## Roles

| Rol | Usuario | Contraseña | Permisos |
|---|---|---|---|
| Admin | admin@parqueadero.com | Admin123* | Todo: panel, historial/recaudo, tarifas |
| Operador | operador@parqueadero.com | Operador123* | Panel: entradas y salidas |

## Ejecución

Requisitos: .NET 8 SDK (la app de escritorio requiere Windows).

```bash
dotnet restore
dotnet run --project src/Parqueadero.Web        # web
dotnet run --project src/Parqueadero.Desktop    # escritorio (Windows)
```

La base de datos y los usuarios se crean automáticamente al primer arranque.

## Publicar en GitHub

```bash
git init && git add . && git commit -m "Miniproyecto 2 - Parqueadero"
git branch -M main
git remote add origin https://github.com/<usuario>/parqueadero.git
git push -u origin main
```

## Guion de la exposición (15 min)
1. Problemática · 2. Justificación · 3. Metodología (capas, EF Core, Identity) · 4. Demo (web en celular/tablet, escritorio, roles) · 5. Conclusiones.
