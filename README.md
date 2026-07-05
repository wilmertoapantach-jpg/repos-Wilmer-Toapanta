🚀 Gestión de Usuarios y Tareas (userManagement & workManagement)
Este repositorio contiene las APIs del Backend y los scripts de SQL Server necesarios para inicializar las bases de datos encargadas de la gestión de usuarios y el control de flujos de trabajo o tareas (workItems).

📋 Requisitos Previos
Motor de Base de Datos: SQL Server.

Herramientas de Desarrollo: Visual Studio 2022 y .NET 9.

Administrador de BD: SQL Server Management Studio (SSMS).

🔧 Configuración Inicial (Connection Strings)
Antes de levantar los proyectos, es necesario configurar la cadena de conexión a las bases de datos. Para ello, dirígete al archivo appsettings.json de cada proyecto backend, busca la sección de conexión y cambia el host, usuario y password según tu entorno local:

{
"ConnectionStrings": {
"DefaultConnection": "Server=TU_SERVER;Database=userManagement;User Id=TU_USUARIO;Password=TU_PASSWORD;TrustServerCertificate=True;"
}
}

📐 Modelo de Datos y Estructura
El proyecto utiliza dos bases de datos independientes:

1. Base de Datos: userManagement
Almacena la información de los usuarios del sistema.

Tabla userData: Registra la identidad, datos de contacto y estado actual de cada usuario.

2. Base de Datos: workManagement
Gestiona las asignaciones y tareas pendientes.

Tabla workItem: Registra las tareas, descripciones, fechas de vencimiento, prioridades y el usuario asignado (assignedUserId).

Script de Inicialización SQL
Ejecuta este script en tu instancia de SQL Server para crear la estructura de datos completa:

-- =======================================================
-- 1. CREACIÓN DE LA BASE DE DATOS DE USUARIOS
-- =======================================================
CREATE DATABASE [userManagement];
GO
USE [userManagement];
GO

CREATE TABLE [userData](
[id] [int] IDENTITY(1,1) NOT NULL,
[identificationNumber] nvarchar NOT NULL,
[fullName] nvarchar NOT NULL,
[email] nvarchar NOT NULL,
[status] [smallint] NOT NULL,
CONSTRAINT [PK_userData] PRIMARY KEY CLUSTERED ([id] ASC)
);
GO

-- =======================================================
-- 2. CREACIÓN DE LA BASE DE DATOS DE TRABAJO/TAREAS
-- =======================================================
CREATE DATABASE [workManagement];
GO
USE [workManagement];
GO

CREATE TABLE [dbo].[workItem](
[id] [int] IDENTITY(1,1) NOT NULL,
[title] nvarchar NOT NULL,
[description] nvarchar NULL,
[status] [smallint] NOT NULL,
[dueDate] [datetime] NOT NULL,
[priority] [int] NOT NULL,
[assignedUserId] [int] NULL,
[createdDate] [datetime] NOT NULL,
CONSTRAINT [PK_workItem] PRIMARY KEY CLUSTERED ([id] ASC)
);
GO

👥 Backend de Usuarios
Este componente consta de tres endpoints principales:

1. Crear / Actualizar Usuarios
Endpoint: POST api/User/SaveUser

Request Body:
{
"id": 0,
"identificationNumber": "180431859",
"fullName": "UsuarioA",
"email": "UsuarioA@hotmail.com",
"status": 1
}
(Nota: Envía id: 0 para crear un nuevo usuario o el ID correspondiente para actualizar uno existente).

2. Lista de Usuarios con Paginación
Endpoint: POST api/User/ListUsers

Request Body:
{
"pageNumber": 1,
"pageSize": 10
}

3. Lista de Usuarios sin Paginación
Endpoint: POST api/User/ListAllUsers

Request Body:
{
"status": 1
}

📝 Backend de Gestión de Tareas
Este componente consta de tres endpoints principales:

1. Creación de Item de Trabajo
Endpoint: POST api/WorkItem/SaveWorkItem

Request Body:
{
"workItemId": 0,
"title": "Ejemplo tarea",
"description": "Ejemplo descripción tarea",
"dueDate": "2026-07-06T17:16:38.320Z",
"priority": 1,
"status": 1
}

💡 Reglas de Negocio:

Prioridades (priority): 0 = Baja | 1 = Alta

Estados (status): 1 = Pendiente | 2 = Completado

2. Lista de Actividades de Trabajo por Usuario
Permite obtener la lista de tareas asignadas de manera paginada filtrando por el ID del usuario.

Endpoint: POST api/WorkItem/ListWorkItemsByUser

Request Body:
{
"pageNumber": 1,
"pageSize": 10,
"assignedUserId": 1
}

3. Reasignación de Actividad
Permite cambiar el usuario asignado a una tarea específica a través del identificador de la actividad.

Endpoint: PUT api/WorkItem/AssignWorkItem

Request Body:
{
"workItemId": 1,
"assignedUserId": 2
}