🚀 Gestión de Usuarios y Tareas (userManagement & workManagement)
Este repositorio contiene las APIs del Backend y los scripts de SQL Server necesarios para inicializar las bases de datos encargadas de la gestión de usuarios y el control de flujos de trabajo o tareas (workItems).
Arquitecturas
-Microservicios
-Hexagonal

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
[identificationNumber] nvarchar(20) NOT NULL,
[fullName] nvarchar(200) NOT NULL,
[email] nvarchar(150) NOT NULL,
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
[title] nvarchar(200) NOT NULL,
[description] nvarchar(500) NULL,
[status] [smallint] NOT NULL,
[dueDate] [datetime] NOT NULL,
[priority] [int] NOT NULL,
[assignedUserName] nvarchar(200) NULL,
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
Permite obtener la lista de tareas asignadas de manera paginada filtrando por usuario y acorde a la fecha y la prioridad.

Endpoint: POST api/WorkItem/ListWorkItems

Request Body:
{
  "pageNumber": 1,
  "pageSize": 10,
  "status": 1,
  "assignedUserName": "usuario A"
}

3. Reasignación de Actividad
Permite cambiar el usuario asignado a una tarea específica a través del identificador de la actividad.

Endpoint: PUT api/WorkItem/AssignWorkItem

Request Body:
{
"workItemId": 1
}


Ejemplos practicos

1. Regla de Urgencia (Fecha menor a 3 días)
Esta regla ignora la relevancia y busca directamente al usuario con menos tareas totales.

Caso 1: Tarea Urgente de Baja Relevancia

Contexto: Vence mañana (menos de 3 días). Debe ir al usuario con menos ítems de trabajo.

JSON:

{
  "workItemId": 0,
  "title": "Corrección de Bug de Interfaz",
  "description": "El botón de cierre del modal-overlay no responde en dispositivos móviles.",
  "dueDate": "2026-07-08T18:00:00.000Z",
  "priority": 0,
  "status": 1
}

Caso 2: Tarea Urgente de Alta Relevancia

Contexto: Vence en dos días. Aunque sea alta, la prioridad de asignación la maneja el vencimiento próximo.

JSON:

{
  "workItemId": 0,
  "title": "Caída del servicio de Firmas",
  "description": "Fallo crítico en el ambiente de producción al firmar documentos.",
  "dueDate": "2026-07-09T09:30:00.000Z",
  "priority": 1,
  "status": 1
}

2. Regla de Relevancia (Tareas No Urgentes: mayor o igual a 3 días)
Se evalúa primero la prioridad (1 antes que 0) y se distribuye al usuario con menor lista de pendientes.

Caso 3: Tarea No Urgente - Alta Relevancia

Contexto: Entrega lejana (13 días). Debe asignarse con prioridad sobre las bajas a los usuarios que tengan espacio (no saturados).

JSON:

{
  "workItemId": 0,
  "title": "Integración Pasarela de Pagos",
  "description": "Desarrollo del módulo de comunicación con el Core Bancario.",
  "dueDate": "2026-07-20T23:59:59.000Z",
  "priority": 1,
  "status": 1
}
Caso 4: Tarea No Urgente - Baja Relevancia

Contexto: Entrega lejana (18 días). Se procesará al final o se asignará al usuario que tenga la menor carga de pendientes actual.

JSON:
{
  "workItemId": 0,
  "title": "Refactorización de Consultas SQL",
  "description": "Optimizar los índices de la tabla de logs en PostgreSQL.",
  "dueDate": "2026-07-25T12:00:00.000Z",
  "priority": 0,
  "status": 1
}

3. Reglas de Control Especiales
Caso 5: Inyección a un Entorno Saturado

Contexto: Usas este JSON para validar que, si un usuario ya cuenta con más de 3 tareas con priority: 1, el sistema lo salte automáticamente y no le asigne este ítem.


JSON
{
  "workItemId": 0,
  "title": "Nueva Feature de Alta Relevancia",
  "description": "Validar que el algoritmo de asignación excluya a los usuarios saturados.",
  "dueDate": "2026-07-15T15:00:00.000Z",
  "priority": 1,
  "status": 1
}
Caso 6: Frontera de Tiempo Exacta (Edge Case de 72 horas)

Contexto: Como la fecha actual simulada es 2026-07-07, este JSON vence exactamente el 2026-07-10. Sirve para verificar si tu lógica toma los 3 días como un menor estricto (<) o menor o igual (<=).

JSON:

{
  "workItemId": 0,
  "title": "Verificación Límite de 3 días",
  "description": "Validar comportamiento exacto en la frontera de las 72 horas.",
  "dueDate": "2026-07-10T00:00:00.000Z",
  "priority": 0,
  "status": 1
}