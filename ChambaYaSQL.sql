USE master
GO
DROP DATABASE IF EXISTS BD_ChambaYa
GO
CREATE DATABASE BD_ChambaYa
GO
USE BD_ChambaYa
GO

-- ==========================================
-- 1. CREACIÓN DE TABLAS
-- ==========================================
CREATE TABLE Rol (
    IdRol INT IDENTITY PRIMARY KEY,
    NombreRol VARCHAR(50) NOT NULL
)
GO

CREATE TABLE Usuario (
    IdUsuario INT IDENTITY PRIMARY KEY,
    IdRol INT REFERENCES Rol(IdRol),
    Nombres VARCHAR(100) NOT NULL,
    Email VARCHAR(100) UNIQUE NOT NULL,
    Clave VARCHAR(255) NOT NULL, 
    Activo BIT DEFAULT 1
)
GO

CREATE TABLE Categoria (
    IdCategoria INT IDENTITY PRIMARY KEY,
    NombreCategoria VARCHAR(50) NOT NULL
)
GO

CREATE TABLE Oferta (
    IdOferta INT IDENTITY PRIMARY KEY,
    IdCategoria INT REFERENCES Categoria(IdCategoria),
    IdUsuario INT REFERENCES Usuario(IdUsuario),
    Titulo VARCHAR(100) NOT NULL,
    Descripcion VARCHAR(MAX) NOT NULL,
    Salario MONEY NOT NULL,
    Ubicacion VARCHAR(100) NOT NULL,
    Modalidad VARCHAR(50) NOT NULL,
    Requisitos VARCHAR(MAX) NOT NULL,
    FechaPublicacion DATETIME DEFAULT GETDATE(),
    Activa BIT DEFAULT 1
)
GO

CREATE TABLE PostulacionCabecera (
    IdPostulacion INT IDENTITY PRIMARY KEY,
    IdUsuario INT REFERENCES Usuario(IdUsuario),
    FechaPostulacion DATETIME DEFAULT GETDATE()
)
GO

CREATE TABLE PostulacionDetalle (
    IdDetalle INT IDENTITY PRIMARY KEY,
    IdPostulacion INT REFERENCES PostulacionCabecera(IdPostulacion),
    IdOferta INT REFERENCES Oferta(IdOferta),
    Estado VARCHAR(30) DEFAULT 'En Revisión', -- Aprobado, Rechazado
    MensajeRespuesta VARCHAR(MAX) NULL
)
GO

-- ==========================================
-- 2. INSERCIÓN DE DATOS
-- ==========================================
INSERT INTO Rol (NombreRol) VALUES ('Administrador'), ('Postulante')

INSERT INTO Usuario (IdRol, Nombres, Email, Clave) VALUES 
(1, 'Admin General', 'admin@chambaya.pe', 'admin123'),
(1, 'Reclutador Tech', 'reclutador@chambaya.pe', 'admin123'),
(2, 'Michael Postulante', 'michael@chamba.com', '123456'),
(2, 'Victor Postulante', 'victor@chamba.com', '123456'),
(2, 'Maria Postulante', 'maria@chamba.com', '123456')

INSERT INTO Categoria (NombreCategoria) VALUES 
('Tecnología'), ('Ventas'), ('Atención al Cliente'), ('Diseño'), ('Administración')
GO

INSERT INTO Oferta (IdCategoria, IdUsuario, Titulo, Descripcion, Salario, Ubicacion, Modalidad, Requisitos) VALUES 
(1, 1, 'Desarrollador Junior .NET', 'Mantenimiento de sistemas MVC y APIs', 1500, 'Lima', 'Remoto', 'Conocimientos en C#, SQL Server, y ganas de aprender.'),
(1, 1, 'Analista de QA', 'Pruebas manuales y automatizadas', 1800, 'Arequipa', 'Híbrido', 'Experiencia en Selenium y Postman.'),
(2, 1, 'Vendedor de Mostrador', 'Atención en tienda de tecnología', 1025, 'Lima', 'Presencial', 'Carisma y disponibilidad para fines de semana.'),
(2, 1, 'Ejecutivo de Cuentas', 'Gestión de cartera B2B', 2500, 'Trujillo', 'Híbrido', '2 años de experiencia en ventas corporativas.'),
(3, 1, 'Asesor de Call Center', 'Soporte técnico a clientes', 1200, 'Lima', 'Remoto', 'Internet estable y buena dicción.'),
(3, 1, 'Recepcionista Bilingüe', 'Atención en lobby de hotel', 1600, 'Cusco', 'Presencial', 'Inglés avanzado indispensable.'),
(4, 1, 'Diseñador UX/UI', 'Diseño de interfaces web y móviles', 3000, 'Lima', 'Remoto', 'Figma, Adobe XD, Portafolio actualizado.'),
(4, 1, 'Community Manager', 'Gestión de redes sociales', 1400, 'Piura', 'Híbrido', 'Conocimiento en Meta Ads y creación de contenido.'),
(5, 1, 'Asistente Administrativo', 'Gestión documentaria y planillas', 1300, 'Chiclayo', 'Presencial', 'Excel intermedio y orden.'),
(5, 1, 'Analista Financiero', 'Proyecciones y flujos de caja', 3500, 'Lima', 'Híbrido', 'Bachiller en Economía o Contabilidad.')
GO

-- ==========================================
-- 3. PROCEDIMIENTOS ALMACENADOS
-- ==========================================
CREATE OR ALTER PROCEDURE sp_listar_ofertas_activas
AS
BEGIN
    SELECT 
        O.IdOferta, C.NombreCategoria, O.Titulo, O.Descripcion, 
        O.Salario, O.Ubicacion, O.Modalidad, O.Requisitos, O.FechaPublicacion,
        O.IdCategoria 
    FROM Oferta O
    INNER JOIN Categoria C ON O.IdCategoria = C.IdCategoria
    WHERE O.Activa = 1
    ORDER BY O.FechaPublicacion DESC
END
GO

CREATE OR ALTER PROCEDURE sp_listar_categorias
AS
BEGIN
    SELECT IdCategoria, NombreCategoria FROM Categoria;
END
GO

CREATE OR ALTER PROCEDURE sp_insertar_oferta
    @IdCategoria INT, @IdUsuario INT, @Titulo VARCHAR(100), @Descripcion VARCHAR(MAX), @Salario MONEY, @Ubicacion VARCHAR(100), @Modalidad VARCHAR(50), @Requisitos VARCHAR(MAX)
AS
BEGIN
    INSERT INTO Oferta (IdCategoria, IdUsuario, Titulo, Descripcion, Salario, Ubicacion, Modalidad, Requisitos, FechaPublicacion, Activa)
    VALUES (@IdCategoria, @IdUsuario, @Titulo, @Descripcion, @Salario, @Ubicacion, @Modalidad, @Requisitos, GETDATE(), 1);
END
GO

CREATE OR ALTER PROCEDURE sp_registrar_postulacion_masiva
    @IdUsuario INT, @OfertasIds VARCHAR(MAX)
AS
BEGIN
    BEGIN TRY
        BEGIN TRAN;
        DECLARE @IdPostulacion INT;
        INSERT INTO PostulacionCabecera (IdUsuario, FechaPostulacion) VALUES (@IdUsuario, GETDATE());
        SET @IdPostulacion = SCOPE_IDENTITY();
        INSERT INTO PostulacionDetalle (IdPostulacion, IdOferta, Estado)
        SELECT @IdPostulacion, value, 'En Revisión' FROM STRING_SPLIT(@OfertasIds, ',');
        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        ROLLBACK TRAN;
        THROW;
    END CATCH
END
GO

-- Admin: Ver ofertas creadas
CREATE OR ALTER PROCEDURE sp_listar_mis_ofertas_admin
    @IdUsuario INT
AS
BEGIN
    SELECT 
        O.IdOferta, O.Titulo, O.FechaPublicacion, O.Activa,
        (SELECT COUNT(1) FROM PostulacionDetalle PD WHERE PD.IdOferta = O.IdOferta) AS TotalPostulantes
    FROM Oferta O
    WHERE O.IdUsuario = @IdUsuario
    ORDER BY O.FechaPublicacion DESC
END
GO

-- Admin: Ver postulantes de una oferta
CREATE OR ALTER PROCEDURE sp_listar_postulantes_por_oferta
    @IdOferta INT
AS
BEGIN
    SELECT 
        PD.IdDetalle,
        U.Nombres, 
        U.Email, 
        PC.FechaPostulacion, 
        PD.Estado,
        PD.MensajeRespuesta
    FROM PostulacionDetalle PD
    INNER JOIN PostulacionCabecera PC ON PD.IdPostulacion = PC.IdPostulacion
    INNER JOIN Usuario U ON PC.IdUsuario = U.IdUsuario
    WHERE PD.IdOferta = @IdOferta
    ORDER BY PC.FechaPostulacion DESC
END
GO

-- Admin: Aprobar/Rechazar postulante
CREATE OR ALTER PROCEDURE sp_evaluar_postulante
    @IdDetalle INT,
    @Estado VARCHAR(30),
    @Mensaje VARCHAR(MAX)
AS
BEGIN
    UPDATE PostulacionDetalle
    SET Estado = @Estado, MensajeRespuesta = @Mensaje
    WHERE IdDetalle = @IdDetalle
END
GO

-- Postulante: Ver el estado de sus postulaciones
CREATE OR ALTER PROCEDURE sp_mis_postulaciones
    @IdUsuario INT
AS
BEGIN
    SELECT 
        O.Titulo,
        C.NombreCategoria,
        O.Ubicacion,
        O.Salario,
        PC.FechaPostulacion,
        PD.Estado,
        PD.MensajeRespuesta
    FROM PostulacionDetalle PD
    INNER JOIN PostulacionCabecera PC ON PD.IdPostulacion = PC.IdPostulacion
    INNER JOIN Oferta O ON PD.IdOferta = O.IdOferta
    INNER JOIN Categoria C ON O.IdCategoria = C.IdCategoria
    WHERE PC.IdUsuario = @IdUsuario
    ORDER BY PC.FechaPostulacion DESC
END
GO


CREATE OR ALTER PROCEDURE sp_actualizar_oferta
    @IdOferta INT, @IdCategoria INT, @Titulo VARCHAR(100), @Descripcion VARCHAR(MAX), @Salario MONEY, @Ubicacion VARCHAR(100), @Modalidad VARCHAR(50), @Requisitos VARCHAR(MAX)
AS BEGIN
    UPDATE Oferta SET IdCategoria = @IdCategoria, Titulo = @Titulo, Descripcion = @Descripcion, Salario = @Salario, Ubicacion = @Ubicacion, Modalidad = @Modalidad, Requisitos = @Requisitos WHERE IdOferta = @IdOferta;
END
GO
CREATE OR ALTER PROCEDURE sp_eliminar_oferta
    @IdOferta INT
AS BEGIN
    UPDATE Oferta SET Activa = 0 WHERE IdOferta = @IdOferta;
END
GO
CREATE OR ALTER PROCEDURE sp_obtener_oferta
    @IdOferta INT
AS BEGIN
    SELECT IdOferta, IdCategoria, IdUsuario, Titulo, Descripcion, Salario, Ubicacion, Modalidad, Requisitos, FechaPublicacion, Activa FROM Oferta WHERE IdOferta = @IdOferta;
END
GO
