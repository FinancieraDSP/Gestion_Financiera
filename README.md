# 💰 Gestión Financiera

Sistema web para la administración de finanzas personales desarrollado con ASP.NET Core, C#, Entity Framework y SQL Server.

Gestión Financiera permite a los usuarios registrar sus ingresos, gastos, presupuestos, deudas y metas de ahorro desde una sola plataforma. Además, incorpora recomendaciones inteligentes para ayudar a mejorar la toma de decisiones financieras.

La aplicación está **separada en backend (API) y frontend (Web)**, **contenerizada con Docker** y **publicada en Microsoft Azure**, de modo que se puede acceder desde cualquier lugar y en cualquier momento.

---

# 📖 Descripción

El objetivo principal de la aplicación es brindar una herramienta sencilla e intuitiva para que los usuarios puedan controlar sus finanzas personales, visualizar su situación económica y planificar mejor sus recursos.

El sistema cuenta con dos tipos de acceso:

- Usuario
- Administrador

Cada rol posee funcionalidades específicas adaptadas a sus necesidades.

---

# ✨ Funcionalidades

## 👤 Usuario

### Dashboard
- Visualización de saldo disponible
- Ingresos del mes
- Gastos del mes
- Ahorro mensual
- Gastos por categoría
- Últimos movimientos registrados

### Gestión de ingresos
- Registrar ingresos
- Consultar historial de ingresos
- Eliminar registros

### Gestión de gastos
- Registrar gastos
- Consultar historial de gastos
- Eliminar registros

### Categorías personalizadas
- Crear categorías propias
- Editar categorías
- Desactivar y reactivar categorías

### Presupuestos
- Definir presupuestos mensuales por categoría
- Seguimiento del uso de presupuesto
- Alertas cuando el límite está por alcanzarse

### Metas de ahorro
- Crear metas de ahorro
- Registrar aportes
- Monitorear progreso
- Eliminar metas

### Gestión de deudas
- Registrar deudas
- Registrar pagos
- Monitorear vencimientos
- Consultar historial de pagos

### Notificaciones
- Notificaciones automáticas relacionadas con:
  - Presupuestos
  - Deudas
  - Ahorro
  - Eventos financieros importantes

### Asistente IA 🚧 (en desarrollo)
- Recomendaciones financieras personalizadas
- Chat financiero basado en información registrada por el usuario
- Se integrará con Azure OpenAI (ver la sección [Asistente IA](#-asistente-ia-pendiente))

### Seguridad
- Cambio de contraseña
- Recuperación de contraseña por correo electrónico
- Control de acceso por autenticación

---

## 🛡️ Administrador

### Panel general
- Resumen del sistema
- Estadísticas generales

### Gestión de usuarios
- Visualización de usuarios registrados
- Bloqueo y desbloqueo de cuentas
- Eliminación de usuarios

> El administrador **no** puede ver ni restablecer contraseñas: cada usuario recupera la suya por correo.

### Gestión de categorías generales
- Crear categorías globales
- Editar categorías globales
- Activar y desactivar categorías

### Configuración del sistema
- Umbrales de notificación
- Parámetros generales
- Opciones de mantenimiento
- Configuración de seguridad

---

# 🏗️ Arquitectura del proyecto

El proyecto está dividido en **tres proyectos** dentro de una misma solución (`GestionFinanciera.slnx`). El frontend conserva el patrón MVC con vistas Razor; toda la lógica de negocio y el acceso a datos viven en el backend.

```text
Navegador ──HTTPS──▶ Web (ASP.NET Core MVC) ──HTTPS + JWT──▶ API (ASP.NET Core Web API) ──▶ Azure SQL Database
                          │
                          └── llaves de sesión ──▶ Azure Blob Storage
```

```text
Gestion_Financiera
│
├── frontend
│   └── GestionFinanciera.Web        ← Interfaz (vistas Razor). No accede a la base de datos.
│       ├── Areas/Admin              ← Panel del administrador
│       ├── Controllers              ← Llaman a la API mediante ApiClient
│       ├── Filtros
│       ├── Servicios                ← ApiClient (cliente de la API)
│       ├── ViewComponents
│       ├── Views
│       ├── wwwroot (css, js, lib)
│       ├── Dockerfile
│       └── Program.cs
│
├── backend
│   └── GestionFinanciera.Api        ← Lógica de negocio, EF Core, JWT y correo
│       ├── Controllers (y Controllers/Admin)
│       ├── Datos                    ← DbContext, inicializador y actualizador de esquema
│       ├── Filtros
│       ├── Servicios
│       ├── Dockerfile
│       └── Program.cs
│
├── shared
│   └── GestionFinanciera.Shared     ← Entidades y ViewModels usados por ambos proyectos
│
├── database
│   └── FinanzasPersonales.sql       ← Esquema (la API lo ejecuta sola al arrancar)
│
├── scripts
│   └── desplegar.ps1                ← Publicación de nuevas versiones en Azure
│
├── docker-compose.yml
├── .env.example
└── GestionFinanciera.slnx
```

## 🔄 Cómo se comunican

1. El usuario inicia sesión en el **Web**, que envía las credenciales a la **API** (`POST /api/auth/login`).
2. La API valida y devuelve un **token JWT** (60 minutos).
3. El Web guarda el token dentro de su **cookie de sesión cifrada** y lo envía en cada llamada a la API (`Authorization: Bearer`).
4. La API valida el token, el rol y que la cuenta siga activa antes de responder.

---

# 🛠️ Tecnologías utilizadas

## Backend

- ASP.NET Core Web API
- C#
- .NET 10
- Entity Framework Core
- SQL Server / Azure SQL Database
- Autenticación JWT
- MailKit (correo SMTP)
- Serilog (registros)

## Frontend

- ASP.NET Core MVC (vistas Razor)
- HTML5
- CSS3
- JavaScript
- Tabler Icons

## Infraestructura y nube

- Docker y Docker Compose
- Azure App Service (Linux)
- Azure SQL Database
- Azure Container Registry
- Azure Blob Storage
- Azure OpenAI Service (en desarrollo)

## Herramientas

- Visual Studio 2026
- Git
- GitHub
- Docker Desktop
- Azure CLI

---

# 🗄️ Base de datos

La aplicación utiliza SQL Server (local) y **Azure SQL Database** (nube) para el almacenamiento de información relacionada con:

- Usuarios
- Roles
- Ingresos
- Gastos
- Categorías
- Presupuestos
- Metas de ahorro
- Deudas
- Pagos de deuda
- Recomendaciones
- Notificaciones

## Creación automática y datos iniciales

Al arrancar, la API crea las tablas si la base está vacía (`database/FinanzasPersonales.sql`) y aplica las actualizaciones de esquema pendientes. En Azure la base la crea el portal y la API solo crea las tablas, porque Azure SQL no admite `CREATE DATABASE` ni `USE`.

La base **no contiene datos de ejemplo**. Solo se siembra lo mínimo para poder usar la aplicación:

- Los roles (Administrador y Usuario)
- Las categorías generales (7 de gasto y 3 de ingreso)
- Un **administrador inicial** y un **cliente inicial** (con dos cuentas en $0), cuyos datos se leen de la configuración (`SeedUsuarios`) y no del código

Todo lo demás (movimientos, presupuestos, metas, deudas) lo registran los usuarios. Los usuarios iniciales solo se crean si la base **no tiene ningún usuario**.

---

# 🚀 Instalación

## Requisitos previos

- Visual Studio 2026
- .NET 10 SDK
- SQL Server o LocalDB
- Git
- Docker Desktop (opcional, para ejecutar con contenedores)
- Azure CLI (solo para quien despliegue en Azure)

---

## 1. Clonar el repositorio

```bash
git clone https://github.com/FinancieraDSP/Gestion_Financiera.git
```

---

## 2. Abrir la solución

Abrir el archivo:

```text
GestionFinanciera.slnx
```

en Visual Studio.

---

## 3. Configurar la API

Copiar el archivo de ejemplo y completar sus valores:

```text
backend/GestionFinanciera.Api/appsettings.Development.example.json
→ backend/GestionFinanciera.Api/appsettings.Development.json
```

Este archivo contiene la clave del JWT, los usuarios iniciales y el correo SMTP, y **no se sube a git**. Debe incluir como mínimo:

```json
{
  "Jwt": { "Key": "cadena-aleatoria-de-minimo-48-caracteres" },
  "SeedUsuarios": {
    "Admin":   { "Nombre": "Administrador",   "Correo": "admin@local.test",   "Contrasena": "..." },
    "Cliente": { "Nombre": "Cliente Inicial", "Correo": "cliente@local.test", "Contrasena": "..." }
  },
  "EmailSettings": { "SmtpServer": "...", "Port": 587, "SenderEmail": "...", "SenderName": "...", "Password": "..." }
}
```

Si no se define `ConnectionStrings`, la API usa **LocalDB** (valor de `backend/GestionFinanciera.Api/appsettings.json`) y crea la base local automáticamente. Para usar otra base, agregar el bloque `ConnectionStrings:DefaultConnection` en `appsettings.Development.json`.

---

## 4. Restaurar paquetes

```bash
dotnet restore GestionFinanciera.slnx
```

---

## 5. Compilar el proyecto

```bash
dotnet build GestionFinanciera.slnx
```

o utilizar:

```text
Ctrl + Shift + B
```

desde Visual Studio.

---

## 6. Ejecutar el proyecto

Se deben ejecutar **la API y el Web** a la vez:

```bash
dotnet run --project backend/GestionFinanciera.Api      # http://localhost:5100
dotnet run --project frontend/GestionFinanciera.Web     # http://localhost:5224
```

o, en Visual Studio, *Configurar proyectos de inicio → Varios proyectos* (API y Web) y utilizar:

```text
F5
```

La aplicación se abre en `http://localhost:5224`. Para verificar la API: `http://localhost:5100/health`.

---

# ⚙️ Configuración (variables de entorno)

La configuración sensible nunca va en el código. Los `__` equivalen a `:` en la configuración de .NET. En Azure se cargan en *App Service → Environment variables*; en Docker, desde el archivo `.env`.

| Variable | Sitio | Para qué |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | API | Cadena de conexión a SQL Server / Azure SQL |
| `Jwt__Key` | API | Firma de los tokens (mínimo 48 caracteres; **distinta en cada entorno**) |
| `Frontend__BaseUrl` | API | URL pública del Web (se usa en el enlace del correo de recuperación) |
| `EmailSettings__SmtpServer`, `__Port`, `__SenderEmail`, `__SenderName`, `__Password` | API | Correo SMTP |
| `SeedUsuarios__Admin__*`, `SeedUsuarios__Cliente__*` | API | Usuarios iniciales; solo se usan en el primer arranque sobre una base sin usuarios |
| `Api__BaseUrl` | Web | URL de la API, con `/` al final |
| `DataProtection__BlobConnectionString` | Web | Llaves de sesión en Blob Storage (sin esto se guardan en disco) |
| `WEBSITES_PORT=8080` | API y Web | Puerto del contenedor (Azure App Service) |
| `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` | API y Web | Reconoce el HTTPS que termina Azure |
| `ASPNETCORE_ENVIRONMENT=Production` | API y Web | Entorno de ejecución |

**Nunca** se suben a git contraseñas, cadenas de conexión ni claves: `.env` y `appsettings.Development.json` están en el `.gitignore`.

---

# 🐳 Contenerización con Docker

Cada proyecto tiene su propio `Dockerfile` (compilación en dos etapas, imagen `aspnet:10.0`, usuario sin privilegios y zona horaria `America/El_Salvador`). El archivo `docker-compose.yml` levanta la API y el Web conectados entre sí.

1. Copiar `.env.example` como `.env` y completar sus valores (cadena de conexión, clave JWT y correo).
2. Ejecutar desde la raíz del repositorio:

```bash
docker compose up -d --build
```

| Servicio | Dirección |
|---|---|
| Web | http://localhost:5224 |
| API | http://localhost:5100 (salud: `/health`) |

Comandos útiles:

```bash
docker compose logs -f api     # ver registros de la API
docker compose down            # apagar todo
```

> La cadena de conexión debe ser alcanzable desde el contenedor (por ejemplo Azure SQL con la IP del equipo autorizada en el firewall). LocalDB no funciona dentro de Docker.

Notas:
- Las imágenes usan la variante completa de Debian/Ubuntu (no Alpine ni *chiseled*) porque la aplicación necesita globalización (formato de moneda) y zonas horarias.
- En local, las llaves de sesión del Web se guardan en un volumen de Docker para que sobrevivan a los reinicios.

---

# ☁️ Despliegue en Azure

## Estrategia

| Paso | Descripción | Estado |
|---|---|---|
| 1. Desarrollo | ASP.NET Core + Entity Framework Core + SQL Server, con backend y frontend separados | ✅ |
| 2. Contenerización | Imágenes Docker y variables de entorno | ✅ |
| 3. Publicación | Contenedores desplegados en Azure App Service | ✅ |
| 4. Base de datos | Azure SQL Database en la nube, sin datos simulados | ✅ |
| 5. IA | Configuración segura de Azure OpenAI Service | 🚧 |

## Recursos (región West US 3)

| Recurso | Nombre |
|---|---|
| Grupo de recursos | db-finanzas |
| Servidor y base SQL | finanzapp-sql-alvarado / `FinanzasPersonales` |
| Container Registry | `finanzappalvarado` |
| Storage (llaves de sesión) | `finanzappalvaradost` (contenedor `dataprotection`) |
| App Service Plan (Linux B1, compartido por los dos sitios) | `plan-finanzapp` |
| Sitio Web | `finanzapersonal-dsp` |
| Sitio API | `finanzapersonal-dsp-api` |

Los sitios descargan sus imágenes del registro mediante **identidad administrada** (rol `AcrPull`), sin contraseñas. Ambos tienen HTTPS obligatorio y *Always On*.

## Publicar una nueva versión

Con Docker Desktop abierto y `az login` hecho, desde la raíz del repositorio:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/desplegar.ps1 -Version 3
```

Cada publicación usa un **número de versión nuevo** (3, 4, 5...). El script construye las imágenes, las sube al registro y actualiza los sitios. Para ver las versiones ya usadas:

```powershell
az acr repository show-tags -n finanzappalvarado --repository finanzapp-api -o table
```

Opciones: `-Servicio api`, `-Servicio web` o `-Servicio ambos` (por defecto).

## Volver a una versión anterior

```powershell
az webapp config container set -g <GRUPO> -n <SITIO> `
  --container-image-name finanzappalvarado.azurecr.io/finanzapp-web:<N> `
  --container-registry-url https://finanzappalvarado.azurecr.io
```

## Ver registros

```powershell
az webapp log tail -g <GRUPO> -n <SITIO>
```

## Costos y consideraciones

- Los contenedores personalizados requieren como mínimo el plan **B1** (aprox. USD 12–13 al mes). El **Container Registry Basic** cuesta aprox. USD 5 al mes. Los precios cambian: confirmarlos en el portal de Azure.
- Estos recursos **cobran mientras existan**. Detener los sitios no detiene el cobro del plan: solo borrarlo.
- Si la base de Azure SQL es *serverless*, se **pausa** tras un rato sin uso y la primera petición posterior puede tardar hasta cerca de un minuto (la API reintenta la conexión automáticamente).
- Cada integrante que despliegue necesita rol **Contributor** sobre el grupo de recursos. Quien solo programe no necesita acceso a Azure: puede desarrollar con LocalDB.

---

# 🧪 Pruebas realizadas

Se realizaron pruebas de interfaz y adaptabilidad para los siguientes anchos:

- 320 px
- 375 px
- 425 px
- 768 px
- 900 px
- 1366 px

Incluyendo validación de:

- Dashboard
- Tablas
- Formularios
- Menús
- Notificaciones
- Asistente IA
- Panel administrativo

---

# 📱 Diseño Responsive

La aplicación está optimizada para:

- Teléfonos móviles
- Tablets
- Laptops
- Monitores de escritorio

---

# 🔒 Seguridad

El sistema implementa:

- Autenticación basada en cookies en el frontend y en tokens JWT en la API
- Control de acceso por roles, verificado en la API en cada petición
- Si el administrador bloquea o elimina una cuenta, su token deja de ser válido de inmediato
- Bloqueo de cuentas tras un número configurable de intentos fallidos
- Protección de rutas mediante autorización
- Validación de formularios
- Hash de contraseñas utilizando las herramientas de Identity de .NET
- Recuperación de contraseña por correo con enlace de un solo uso, que se guarda cifrado (hash) y vence en 15 minutos
- El administrador no puede ver ni restablecer contraseñas
- Modo de mantenimiento aplicado tanto en el Web como en la API
- Comunicación mediante **HTTPS** (obligatorio en Azure) y conexión cifrada a la base de datos
- Claves, cadenas de conexión y credenciales **fuera del código**, mediante variables de entorno
- Imágenes de contenedor descargadas con identidad administrada, sin contraseñas
- Llaves de sesión protegidas en Azure Blob Storage
- Sin sincronización con cuentas bancarias y sin solicitar credenciales bancarias

---

# 🤖 Asistente IA (pendiente)

Hoy las pantallas `/AsistenteIA/Recomendaciones` y `/AsistenteIA/Chat` muestran un marcador de posición (`frontend/GestionFinanciera.Web/Controllers/AsistenteIAController.cs`). Ya existen y se pueden reutilizar:

- Las vistas `Recomendaciones.cshtml` (lista de `Recomendacion`) y `Chat.cshtml` (lista de `MensajeChat`)
- La tabla y entidad `Recomendacion` (título, contenido, tipo, fecha, vista)
- El modelo `MensajeChat` en `shared`

**Diseño propuesto** (la IA vive en la API y el Web solo la llama, como el resto de módulos):

1. Un `AsistenteIAService` en la API que arme el contexto con los datos reales del usuario autenticado y llame a Azure OpenAI.
2. Endpoints `api/asistente/...` con `[Authorize(Roles = "Usuario")]` para generar recomendaciones y para el chat.
3. En el Web, reemplazar el marcador de posición por llamadas con `ApiClient` (el patrón está en `IngresosController`).
4. Configuración por variables de entorno: `AzureOpenAI__Endpoint`, `AzureOpenAI__Deployment` y `AzureOpenAI__ApiKey`.

**Ética y seguridad** (hallazgo de las entrevistas: 7 de 10 personas desconfían al dar información bancaria o personal):

- Enviar al modelo **solo datos agregados** (totales por categoría, presupuestos, deudas próximas), nunca nombre, correo ni credenciales.
- Informar al usuario qué datos procesa la IA.
- El usuario puede revisar y modificar las sugerencias de la IA.
- No solicitar credenciales ni datos bancarios.

**Antes de programar:** verificar que la suscripción permite crear un recurso de Azure OpenAI y desplegar un modelo (disponibilidad por región y cuota) y estimar el costo por uso.

---

# 🎯 Objetivo académico

Este proyecto fue desarrollado como parte de la asignatura de Desarrollo de Software, aplicando conceptos de:

- Arquitectura MVC
- Separación de backend y frontend (API REST)
- Desarrollo web con .NET
- Bases de datos relacionales
- Contenerización con Docker
- Despliegue en la nube con Microsoft Azure
- Experiencia de usuario (UX)
- Diseño responsive
- Control de acceso y seguridad

---

# 👥 Equipo de desarrollo

Proyecto desarrollado por estudiantes de la asignatura de Desarrollo de Software.

---

# 📌 Estado actual

✅ Gestión de ingresos

✅ Gestión de gastos

✅ Categorías personalizadas

✅ Presupuestos

✅ Metas de ahorro

✅ Gestión de deudas

✅ Notificaciones

✅ Panel administrativo

✅ Diseño responsive

✅ Separación de backend (API) y frontend (Web)

✅ Contenerización con Docker

✅ Base de datos en Azure SQL (sin datos simulados)

✅ Publicación en Azure App Service con HTTPS

🚧 Asistente IA (integración con Azure OpenAI)

🚧 Monitoreo con Azure Monitor

🚧 Mejoras visuales y optimización continua

---

# 📄 Licencia

Este proyecto fue desarrollado con fines académicos y educativos.