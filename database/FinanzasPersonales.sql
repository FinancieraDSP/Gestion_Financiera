
CREATE DATABASE FinanzasPersonales;
GO

USE FinanzasPersonales;
GO

/*TABLAS*/

/*roles*/

CREATE TABLE Roles (
    id_rol INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL UNIQUE,
    descripcion VARCHAR(200) NULL
);
GO

/*usuarios*/
CREATE TABLE Usuarios (
    id_usuario INT IDENTITY(1,1) PRIMARY KEY,
    id_rol INT NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    correo VARCHAR(150) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    fecha_registro DATETIME2 NOT NULL DEFAULT GETDATE(),
    estado BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_Usuarios_Roles
        FOREIGN KEY (id_rol)
        REFERENCES Roles(id_rol)
);
GO

/*cuentas*/

CREATE TABLE Cuentas (
    id_cuenta INT IDENTITY(1,1) PRIMARY KEY,
    id_usuario INT NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    tipo VARCHAR(30) NOT NULL,
    saldo_inicial DECIMAL(18,2) NOT NULL,
    saldo_actual DECIMAL(18,2) NOT NULL,
    fecha_creacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    estado BIT NOT NULL DEFAULT 1,

    CONSTRAINT CK_Cuentas_Tipo
        CHECK (tipo IN ('Efectivo', 'Banco', 'Ahorro', 'Tarjeta')),

    CONSTRAINT CK_Cuentas_SaldoInicial
        CHECK (saldo_inicial >= 0),

    CONSTRAINT CK_Cuentas_SaldoActual
        CHECK (saldo_actual >= 0),

    CONSTRAINT FK_Cuentas_Usuarios
        FOREIGN KEY (id_usuario)
        REFERENCES Usuarios(id_usuario)
);
GO


/*categorias*/

CREATE TABLE Categorias (
    id_categoria INT IDENTITY(1,1) PRIMARY KEY,
    id_usuario INT NULL,
    nombre VARCHAR(100) NOT NULL,
    tipo VARCHAR(20) NOT NULL,
    descripcion VARCHAR(200) NULL,
    estado BIT NOT NULL DEFAULT 1,

    CONSTRAINT CK_Categorias_Tipo
        CHECK (tipo IN ('Ingreso', 'Gasto')),

    CONSTRAINT FK_Categorias_Usuarios
        FOREIGN KEY (id_usuario)
        REFERENCES Usuarios(id_usuario)
);
GO


/*ingresos*/

CREATE TABLE Ingresos (
    id_ingreso INT IDENTITY(1,1) PRIMARY KEY,
    id_usuario INT NOT NULL,
    id_cuenta INT NOT NULL,
    id_categoria INT NOT NULL,
    descripcion VARCHAR(200) NOT NULL,
    monto DECIMAL(18,2) NOT NULL,
    fecha DATE NOT NULL,
    origen VARCHAR(100) NULL,

    CONSTRAINT CK_Ingresos_Monto
        CHECK (monto > 0),

    CONSTRAINT FK_Ingresos_Usuarios
        FOREIGN KEY (id_usuario)
        REFERENCES Usuarios(id_usuario),

    CONSTRAINT FK_Ingresos_Cuentas
        FOREIGN KEY (id_cuenta)
        REFERENCES Cuentas(id_cuenta),

    CONSTRAINT FK_Ingresos_Categorias
        FOREIGN KEY (id_categoria)
        REFERENCES Categorias(id_categoria)
);
GO


/*gastos*/
CREATE TABLE Gastos (
    id_gasto INT IDENTITY(1,1) PRIMARY KEY,
    id_usuario INT NOT NULL,
    id_cuenta INT NOT NULL,
    id_categoria INT NOT NULL,
    descripcion VARCHAR(200) NOT NULL,
    monto DECIMAL(18,2) NOT NULL,
    fecha DATE NOT NULL,
    comercio VARCHAR(150) NULL,

    CONSTRAINT CK_Gastos_Monto
        CHECK (monto > 0),

    CONSTRAINT FK_Gastos_Usuarios
        FOREIGN KEY (id_usuario)
        REFERENCES Usuarios(id_usuario),

    CONSTRAINT FK_Gastos_Cuentas
        FOREIGN KEY (id_cuenta)
        REFERENCES Cuentas(id_cuenta),

    CONSTRAINT FK_Gastos_Categorias
        FOREIGN KEY (id_categoria)
        REFERENCES Categorias(id_categoria)
);
GO


/*presupuestos*/

CREATE TABLE Presupuestos (
    id_presupuesto INT IDENTITY(1,1) PRIMARY KEY,
    id_usuario INT NOT NULL,
    id_categoria INT NOT NULL,
    monto_limite DECIMAL(18,2) NOT NULL,
    monto_gastado DECIMAL(18,2) NOT NULL DEFAULT 0,
    periodo_inicio DATE NOT NULL,
    periodo_fin DATE NOT NULL,
    estado BIT NOT NULL DEFAULT 1,

    CONSTRAINT CK_Presupuestos_MontoLimite
        CHECK (monto_limite > 0),

    CONSTRAINT CK_Presupuestos_MontoGastado
        CHECK (monto_gastado >= 0),

    CONSTRAINT CK_Presupuestos_Fechas
        CHECK (periodo_fin >= periodo_inicio),

    CONSTRAINT FK_Presupuestos_Usuarios
        FOREIGN KEY (id_usuario)
        REFERENCES Usuarios(id_usuario),

    CONSTRAINT FK_Presupuestos_Categorias
        FOREIGN KEY (id_categoria)
        REFERENCES Categorias(id_categoria)
);
GO


/*metas de ahorro*/

CREATE TABLE MetasAhorro (
    id_meta INT IDENTITY(1,1) PRIMARY KEY,
    id_usuario INT NOT NULL,
    id_cuenta INT NULL,
    nombre VARCHAR(100) NOT NULL,
    descripcion VARCHAR(250) NULL,
    icono VARCHAR(50) NOT NULL DEFAULT 'ti-target',
    monto_objetivo DECIMAL(18,2) NOT NULL,
    monto_actual DECIMAL(18,2) NOT NULL DEFAULT 0,
    fecha_limite DATE NULL,
    estado VARCHAR(20) NOT NULL,

    CONSTRAINT CK_Metas_MontoObjetivo
        CHECK (monto_objetivo > 0),

    CONSTRAINT CK_Metas_MontoActual
        CHECK (monto_actual >= 0),

    CONSTRAINT CK_Metas_MontoActualObjetivo
        CHECK (monto_actual <= monto_objetivo),

    CONSTRAINT CK_Metas_Estado
        CHECK (estado IN ('Activa', 'Completada', 'Cancelada')),

    CONSTRAINT FK_Metas_Usuarios
        FOREIGN KEY (id_usuario)
        REFERENCES Usuarios(id_usuario),

    CONSTRAINT FK_Metas_Cuentas
        FOREIGN KEY (id_cuenta)
        REFERENCES Cuentas(id_cuenta)
);
GO

/*notificaciones*/
CREATE TABLE Notificaciones (
    id_notificacion INT IDENTITY(1,1) PRIMARY KEY,
    id_usuario INT NOT NULL,
    titulo VARCHAR(150) NOT NULL,
    mensaje VARCHAR(500) NOT NULL,
    tipo VARCHAR(50) NOT NULL,
    fecha DATETIME2 NOT NULL DEFAULT GETDATE(),
    leida BIT NOT NULL DEFAULT 0,

    CONSTRAINT FK_Notificaciones_Usuarios
        FOREIGN KEY (id_usuario)
        REFERENCES Usuarios(id_usuario)
);
GO


/*recomendaciones*/

CREATE TABLE Recomendaciones (
    id_recomendacion INT IDENTITY(1,1) PRIMARY KEY,
    id_usuario INT NOT NULL,
    id_categoria INT NULL,
    titulo VARCHAR(150) NOT NULL,
    contenido VARCHAR(1000) NOT NULL,
    tipo VARCHAR(50) NOT NULL,
    fecha_generacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    vista BIT NOT NULL DEFAULT 0,

    CONSTRAINT FK_Recomendaciones_Usuarios
        FOREIGN KEY (id_usuario)
        REFERENCES Usuarios(id_usuario),

    CONSTRAINT FK_Recomendaciones_Categorias
        FOREIGN KEY (id_categoria)
        REFERENCES Categorias(id_categoria)
);
GO

/*deudas*/

CREATE TABLE Deudas (
    id_deuda INT IDENTITY(1,1) PRIMARY KEY,
    id_usuario INT NOT NULL,
    id_cuenta INT NULL,
    nombre VARCHAR(150) NOT NULL,
    acreedor VARCHAR(150) NOT NULL,
    monto_total DECIMAL(18,2) NOT NULL,
    monto_pagado DECIMAL(18,2) NOT NULL DEFAULT 0,
    tasa_interes DECIMAL(5,2) NOT NULL DEFAULT 0,
    fecha_inicio DATE NOT NULL,
    fecha_vencimiento DATE NULL,
    estado VARCHAR(20) NOT NULL,

    CONSTRAINT CK_Deudas_MontoTotal
        CHECK (monto_total > 0),

    CONSTRAINT CK_Deudas_MontoPagado
        CHECK (monto_pagado >= 0),

    CONSTRAINT CK_Deudas_MontoPagadoTotal
        CHECK (monto_pagado <= monto_total),

    CONSTRAINT CK_Deudas_TasaInteres
        CHECK (tasa_interes >= 0),

    CONSTRAINT CK_Deudas_Estado
        CHECK (estado IN ('Pendiente', 'En proceso', 'Pagada', 'Vencida')),

    CONSTRAINT FK_Deudas_Usuarios
        FOREIGN KEY (id_usuario)
        REFERENCES Usuarios(id_usuario),

    CONSTRAINT FK_Deudas_Cuentas
        FOREIGN KEY (id_cuenta)
        REFERENCES Cuentas(id_cuenta)
);
GO

/*pagos de deuda*/

CREATE TABLE PagosDeuda (
    id_pago INT IDENTITY(1,1) PRIMARY KEY,
    id_deuda INT NOT NULL,
    id_cuenta INT NOT NULL,
    monto DECIMAL(18,2) NOT NULL,
    fecha_pago DATE NOT NULL,
    descripcion VARCHAR(200) NULL,

    CONSTRAINT CK_PagosDeuda_Monto
        CHECK (monto > 0),

    CONSTRAINT FK_PagosDeuda_Deudas
        FOREIGN KEY (id_deuda)
        REFERENCES Deudas(id_deuda),

    CONSTRAINT FK_PagosDeuda_Cuentas
        FOREIGN KEY (id_cuenta)
        REFERENCES Cuentas(id_cuenta)
);
GO