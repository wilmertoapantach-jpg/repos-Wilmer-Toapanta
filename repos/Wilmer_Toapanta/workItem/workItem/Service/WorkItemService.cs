using workItem.DTO;
using workItem.Repository.IRepository;
using workItem.Service.IService;
using workItem.Shared;

namespace workItem.Service
{
    /// <summary>
    /// Servicio que implementa la lógica de negocio para la gestión de elementos de trabajo.
    /// Incluye la lógica de distribución automática de tareas entre usuarios según reglas de carga.
    /// </summary>
    public class WorkItemService(IWorkItemRepository workItemRepository, IConfiguration configuration) : IWorkItemService
    {
        private readonly IWorkItemRepository _workItemRepository = workItemRepository;
        private readonly string _userApiBaseUrl = configuration.GetValue<string>("urlUser")!;

        // ─── Constantes para la lógica de distribución ────────────────────────────────
        /// <summary>Identificador de relevancia alta para elementos de trabajo</summary>
        private const int HIGH_RELEVANCE = 1;

        /// <summary>Identificador de relevancia baja para elementos de trabajo</summary>
        private const int LOW_RELEVANCE = 0;

        /// <summary>Umbral máximo de tareas de alta relevancia asignadas a un usuario antes de considerarlo saturado</summary>
        private const int SATURATION_THRESHOLD = 3;   // >3 elementos de alta relevancia → saturado

        /// <summary>Días desde hoy para considerar una tarea como próxima a vencer</summary>
        private const int NEAR_DUE_DAYS = 3;           // <3 días desde ahora → próxima a vencer

        // ─── Public API ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Crea o actualiza un elemento de trabajo.
        /// En caso de creación (WorkItemId == 0), el elemento se asigna automáticamente a un usuario
        /// según las reglas de distribución configuradas.
        /// En caso de actualización (WorkItemId > 0), solo se actualiza la información.
        /// </summary>
        /// <param name="request">Datos del elemento de trabajo a crear o actualizar</param>
        /// <returns>DTO con el elemento de trabajo guardado</returns>
        /// <exception cref="Exception">Se lanza si los datos obligatorios no se proporcionan</exception>
        public async Task<WorkItemResponseDTO> SaveWorkItem(WorkItemRequestDTO request)
        {
            // Validaciones de campos obligatorios
            if (request is null) throw new Exception("La información del WorkItem no puede ser nula.");
            if (string.IsNullOrWhiteSpace(request.Title)) throw new Exception("El título del WorkItem es obligatorio.");

            // Si es creación, asigna usuario automáticamente
            if (request.WorkItemId == 0)
                return await NewWorkItem(request);

            // Si es actualización, solo actualiza los datos
            return await UpdateWorkItem(request);
        }

        /// <summary>
        /// Obtiene un listado de elementos de trabajo aplicando filtros opcionales y paginación.
        /// Los filtros se aplican directamente en el repositorio para optimizar la consulta.
        /// </summary>
        /// <param name="filter">Criterios de filtrado y parámetros de paginación</param>
        /// <returns>Página con los elementos de trabajo filtrados</returns>
        public async Task<PageResponseDTO<WorkItemResponseDTO>> ListWorkItems(WorkItemFilterDTO filter)
        {
            var listUser = await GetActiveUsersAsync();
            var listUserDict = listUser.ToDictionary(u => u.Id, u => u.FullName);
            var listItem = await _workItemRepository.ListWorkItems(filter);
            foreach (var item in listItem.Items)
            {
                var userName = listUserDict.TryGetValue(item.AssignedUserId ?? 0, out var name) ? name : null;
                item.AssignedUserName = userName;
            }
            return listItem;
        }

        /// <summary>
        /// Asigna o reasigna un elemento de trabajo existente al usuario más apropiado
        /// aplicando las reglas de distribución de carga.
        /// </summary>
        /// <param name="workItemId">ID del elemento de trabajo a asignar</param>
        /// <returns>DTO con el elemento de trabajo asignado actualizado</returns>
        /// <exception cref="Exception">Se lanza si el ID no es válido o el elemento no existe</exception>
        public async Task<WorkItemResponseDTO> AssignWorkItem(int workItemId)
        {
            // Validar que el ID sea válido
            if (workItemId <= 0) throw new Exception("El Id del WorkItem debe ser mayor a cero.");

            // Verificar que el elemento existe
            var exists = await _workItemRepository.ExistsWorkItem(workItemId);
            if (!exists) throw new Exception($"WorkItem con Id {workItemId} no encontrado.");

            // Obtener los datos actuales del elemento
            var filter = new WorkItemFilterDTO { WorkItemId = workItemId, PageSize = 1 };
            var page = await _workItemRepository.ListWorkItems(filter);
            var item = page.Items.FirstOrDefault()
                ?? throw new Exception($"WorkItem con Id {workItemId} no encontrado.");

            // Calcular la asignación usando las reglas de distribución
            var assignedUserId = await DistributeWorkItem(item.DueDate, item.Priority);

            // Actualizar el elemento con el nuevo usuario asignado
            var updateRequest = new WorkItemRequestDTO
            {
                WorkItemId = workItemId,
                Title = item.Title,
                Description = item.Description,
                DueDate = item.DueDate,
                Priority = item.Priority,
                Status = item.Status
            };
            updateRequest.AssignedUserId = assignedUserId;
            var result = await _workItemRepository.UpdateWorkItem(updateRequest);
            return result;
        }

        // ─── Private Helpers ──────────────────────────────────────────────────────────

        /// <summary>
        /// Crea un nuevo elemento de trabajo asignándolo automáticamente a un usuario.
        /// La asignación se realiza antes de persistir el elemento en la base de datos.
        /// </summary>
        /// <param name="request">Datos del elemento de trabajo a crear</param>
        /// <returns>DTO con el elemento de trabajo creado</returns>
        private async Task<WorkItemResponseDTO> NewWorkItem(WorkItemRequestDTO request)
        {
            // Asignar usuario antes de guardar
            var assignedUserId = await DistributeWorkItem(request.DueDate, request.Priority);
            request.AssignedUserId = assignedUserId;
            var listUser = await GetActiveUsersAsync();
            var listUserDict = listUser.ToDictionary(u => u.Id, u => u.FullName);
            var result = await _workItemRepository.CreateWorkItem(request);
            result.AssignedUserName = listUserDict.TryGetValue(result.AssignedUserId ?? 0, out var name) ? name : null;
            return result;
        }

        /// <summary>
        /// Actualiza un elemento de trabajo existente validando que existe previamente.
        /// </summary>
        /// <param name="request">Datos actualizados del elemento de trabajo</param>
        /// <returns>DTO con el elemento de trabajo actualizado</returns>
        /// <exception cref="Exception">Se lanza si el elemento no existe</exception>
        private async Task<WorkItemResponseDTO> UpdateWorkItem(WorkItemRequestDTO request)
        {
            // Verificar que el elemento existe
            var exists = await _workItemRepository.ExistsWorkItem(request.WorkItemId);
            if (!exists) throw new Exception($"WorkItem con Id {request.WorkItemId} no encontrado.");

            var listUser = await GetActiveUsersAsync();
            var listUserDict = listUser.ToDictionary(u => u.Id, u => u.FullName);
            var result = await _workItemRepository.UpdateWorkItem(request);
            result.AssignedUserName = listUserDict.TryGetValue(result.AssignedUserId ?? 0, out var name) ? name : null;
            return result;
        }

        /// <summary>
        /// Algoritmo central de distribución de tareas.
        /// Determina a qué usuario se debe asignar una tarea basándose en:
        /// 1. Si la fecha de vencimiento es próxima (< 3 días): asigna al usuario con menos tareas
        /// 2. Si es de alta relevancia: asigna al usuario con menos tareas que no esté saturado
        /// 3. Por defecto: asigna al usuario con menos tareas entre los no saturados
        /// </summary>
        /// <param name="dueDate">Fecha de vencimiento de la tarea</param>
        /// <param name="relevance">Relevancia de la tarea (High o Low)</param>
        /// <returns>ID del usuario asignado, o null si no hay usuarios disponibles</returns>
        private async Task<int?> DistributeWorkItem(DateTime dueDate, int Priority)
        {
            try
            {
                // 1. Obtener usuarios activos de la API externa de usuarios
                var users = await GetActiveUsersAsync();
                if (users.Count == 0) return null;

                // 2. Construir instantánea de carga de trabajo para cada usuario
                var workloads = new List<(UserExternalDTO User, int Total, int HighCount)>();
                foreach (var user in users)
                {
                    var items = await _workItemRepository.GetWorkItemsByUser(user.Id);
                    int total = items.Count;                                              // Total de tareas asignadas
                    int high = items.Count(i => i.Priority == HIGH_RELEVANCE);          // Tareas de alta relevancia
                    workloads.Add((user, total, high));
                }

                // 3. Aplicar reglas de asignación
                bool isNearDue = (dueDate - DateTime.UtcNow).TotalDays <= NEAR_DUE_DAYS;

                // Regla 1: Si la tarea es próxima a vencer, asignar al usuario con menos tareas sin importar relevancia
                if (isNearDue)
                {
                    var candidate = workloads.MinBy(w => w.Total);
                    return candidate.User.Id;
                }

                // Regla 2: Si es de alta relevancia, asignar a usuario no saturado con menos tareas
                if (Priority == HIGH_RELEVANCE)
                {
                    var eligible = workloads
                        .Where(w => w.HighCount <= SATURATION_THRESHOLD)  // Excluir saturados
                        .OrderBy(w => w.Total)                             // Ordenar por carga total
                        .ToList();

                    if (eligible.Count > 0)
                        return eligible.First().User.Id;
                }

                // Regla 3: baja relevancia o sin usuarios elegibles para alta relevancia
                // Asignar al usuario no saturado con menos tareas
                var fallback = workloads
                    .Where(w => w.HighCount <= SATURATION_THRESHOLD)
                    .OrderBy(w => w.Total)
                    .FirstOrDefault();

                return fallback.User?.Id;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al distribuir el WorkItem.", ex);
            }

        }

        /// <summary>
        /// Obtiene la lista de usuarios activos llamando a la API externa de usuarios.
        /// Realiza una solicitud POST con los parámetros de filtrado para obtener usuarios con estado activo.
        /// </summary>
        /// <returns>Lista de usuarios activos desde la API externa</returns>
        /// <exception cref="HttpRequestException">Se lanza si la solicitud a la API externa falla</exception>
        private async Task<List<UserExternalDTO>> GetActiveUsersAsync()
        {
            try
            {
                using var client = new HttpClient();
                client.BaseAddress = new Uri(_userApiBaseUrl);
                // Construir el cuerpo de la solicitud para filtrar usuarios activos
                var body = new
                {
                    status = 1  // 1 = activo
                };
                // Realizar la solicitud POST a la API de usuarios
                var response = await client.PostAsJsonAsync("api/User/ListAllUsers", body);
                response.EnsureSuccessStatusCode();
                // Deserializar la respuesta o retornar lista vacía si no hay resultado
                var result = await response.Content.ReadFromJsonAsync<APIResponseDTO<List<UserExternalDTO>>>();
                return result?.Result ?? new List<UserExternalDTO>();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener usuarios activos desde la API externa.", ex);
            }

        }

        /// <summary>
        /// DTO que representa un usuario externo de la API de usuarios.
        /// Coincide con la estructura de respuesta de https://localhost:7098/api/User/ListAllUsers
        /// </summary>
        public class UserExternalDTO
        {
            /// <summary>Identificador único del usuario en el sistema externo</summary>
            public int Id { get; set; }

            /// <summary>Número de identificación del usuario (cédula, pasaporte, etc.)</summary>
            public string? IdentificationNumber { get; set; }

            /// <summary>Nombre completo del usuario</summary>
            public string? FullName { get; set; }

            /// <summary>Estado del usuario (1 = activo, 0 = inactivo)</summary>
            public int Status { get; set; }
        }

    }
}