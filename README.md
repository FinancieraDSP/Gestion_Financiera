# 💰 Gestión Financiera

Sistema web para la administración de finanzas personales desarrollado con ASP.NET MVC, C#, Entity Framework y SQL Server.

Gestión Financiera permite a los usuarios registrar sus ingresos, gastos, presupuestos, deudas y metas de ahorro desde una sola plataforma. Además, incorpora recomendaciones inteligentes para ayudar a mejorar la toma de decisiones financieras.

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

### Asistente IA
- Recomendaciones financieras personalizadas
- Chat financiero basado en información registrada por el usuario

### Seguridad
- Cambio de contraseña
- Control de acceso por autenticación

---

## 🛡️ Administrador

### Panel general
- Resumen del sistema
- Estadísticas generales

### Gestión de usuarios
- Visualización de usuarios registrados
- Bloqueo y desbloqueo de cuentas
- Restablecimiento de contraseñas
- Eliminación de usuarios

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

El proyecto sigue el patrón MVC (Model - View - Controller).

```text
GestionFinancieraPersonal
│
├── Areas
│   └── Admin
│
├── Controllers
│
├── Datos
│
├── Entidades
│
├── Models
│
├── Servicios
│
├── ViewComponents
│
├── Views
│
├── wwwroot
│   ├── css
│   ├── js
│   └── lib
│
└── Program.cs
```

---

# 🛠️ Tecnologías utilizadas

## Backend

- ASP.NET MVC
- C#
- .NET
- Entity Framework Core
- SQL Server

## Frontend

- HTML5
- CSS3
- JavaScript
- Tabler Icons

## Herramientas

- Visual Studio 2026
- Git
- GitHub

---

# 🗄️ Base de datos

La aplicación utiliza SQL Server para el almacenamiento de información relacionada con:

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

---

# 🚀 Instalación

## Requisitos previos

- Visual Studio 2026
- .NET SDK
- SQL Server
- Git

---

## 1. Clonar el repositorio

```bash
git clone https://github.com/USUARIO/REPOSITORIO.git
```

---

## 2. Abrir la solución

Abrir el archivo:

```text
GestionFinancieraPersonal.sln
```

en Visual Studio.

---

## 3. Configurar la base de datos

Actualizar la cadena de conexión en:

```json
appsettings.json
```

o

```json
appsettings.Development.json
```

según el entorno local.

---

## 4. Restaurar paquetes

```bash
dotnet restore
```

---

## 5. Compilar el proyecto

```bash
dotnet build
```

o utilizar:

```text
Ctrl + Shift + B
```

desde Visual Studio.

---

## 6. Ejecutar el proyecto

```bash
dotnet run
```

o utilizar:

```text
F5
```

desde Visual Studio.

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

- Autenticación basada en cookies
- Control de acceso por roles
- Protección de rutas mediante autorización
- Validación de formularios
- Hash de contraseñas utilizando las herramientas de Identity de .NET

---

# 🎯 Objetivo académico

Este proyecto fue desarrollado como parte de la asignatura de Desarrollo de Software, aplicando conceptos de:

- Arquitectura MVC
- Desarrollo web con .NET
- Bases de datos relacionales
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

✅ Asistente IA

✅ Panel administrativo

✅ Diseño responsive

🚧 Mejoras visuales y optimización continua

---

# 📄 Licencia

Este proyecto fue desarrollado con fines académicos y educativos.