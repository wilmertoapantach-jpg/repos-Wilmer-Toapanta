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
    public class WorkItemService(IWorkItemRepository workItemRepository, IUserService userService) : IWorkItemService
    {
        private readonly IWorkItemRepository _workItemRepository = workItemRepository;
        private readonly IUserService _userService = userService;




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
            var listUserDict = listUser.ToDictionary(u => u.FullName, u => u.Id);
            var listItem = await _workItemRepository.ListWorkItems(filter);
            foreach (var item in listItem.Items)
            {
                var userId = listUserDict.TryGetValue(item.AssignedUserName ?? "", out var id) ? id : 0;
                item.AssignedUserId = userId;
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
            var assignedUserName = await DistributeWorkItem(item.DueDate, item.Priority, item?.AssignedUserName ?? "");

            // Actualizar el elemento con el nuevo usuario asignado
            var updateRequest = new WorkItemRequestDTO
            {
                WorkItemId = workItemId,
                Title = item.Title,
                Description = item.Description,
                DueDate = item.DueDate,
                Priority = item.Priority,
                Status = item.Status,
                AssignedUserName = assignedUserName
            };
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
            var assignedUserName = await DistributeWorkItem(request.DueDate, request.Priority, request.AssignedUserName);
            request.AssignedUserName = assignedUserName;
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
        private async Task<string?> DistributeWorkItem(DateTime dueDate, int Priority, string assignedUserName)
        {
            try
            {
                // 1. Obtener usuarios activos de la API externa de usuarios
                var users = await GetActiveUsersAsync();
                if (users == null || users.Count == 0) throw new Exception("No se encontraron usuarios activos para asignar la tarea.");
                if (!string.IsNullOrEmpty(assignedUserName) && !users.Any(u => u.FullName.ToLower().Trim() == assignedUserName.ToLower().Trim())) throw new Exception("El usuario asignado no es válido o no está activo.");

                // Filtrar usuarios que ya tienen más de 3 tareas de alta relevancia asignadas
                var listUserHighRelevance = await _workItemRepository.CountHighRelevanceByUser(users.Select(u => u.FullName.ToLower().Trim()).Distinct().ToList());
                // Excluir usuarios saturados de la lista de candidatos
                users = users.Where(u => !listUserHighRelevance.Any(l => l.AssignedUserName?.ToLower().Trim() == u.FullName?.ToLower().Trim())).ToList();

                if (users.Count == 0) return null;
                // Obtener la lista de nombres de usuario para consultar las tareas asignadas
                var userNames = users.Select(u => u.FullName).ToList();
                // Obtener la lista de tareas asignadas a los usuarios activos
                var listUserTask = await _workItemRepository.GetWorkItemsByUser(userNames);

                // 2. Construir instantánea de carga de trabajo para cada usuario
                var workloads = new List<(UserExternalDTO User, int Total, int HighCount)>();
                foreach (var user in users)
                {
                    var items = listUserTask.Where(i => i.AssignedUserName == user.FullName).ToList();
                    int total = items.Count();                                              // Total de tareas asignadas
                    int high = items.Count(i => i.Priority == Catalog.HIGH_RELEVANCE);          // Tareas de alta relevancia
                    workloads.Add((user, total, high));
                }

                // 3. Aplicar reglas de asignación
                // Verifica que la fecha de entrega este cerca a vencer
                bool isNearDue = (dueDate - DateTime.UtcNow).TotalDays <= Catalog.NEAR_DUE_DAYS;

                // Regla 1: Si la tarea es próxima a vencer, asignar al usuario con menos tareas sin importar relevancia
                if (isNearDue)
                {
                    var candidate = workloads.MinBy(w => w.Total);
                    return candidate.User.FullName;
                }

                // Regla 2: Si es de alta relevancia, asignar a usuario no saturado con menos tareas
                if (Priority == Catalog.HIGH_RELEVANCE)
                {
                    var eligible = workloads
                        .Where(w => w.HighCount <= Catalog.SATURATION_THRESHOLD)  // Excluir saturados
                        .OrderBy(w => w.Total)                             // Ordenar por carga total
                        .ToList();

                    if (eligible.Count > 0)
                        return eligible.First().User.FullName;
                }

                // Regla 3: baja relevancia o sin usuarios elegibles para alta relevancia
                // Asignar al usuario no saturado con menos tareas
                var fallback = workloads
                    .Where(w => w.HighCount <= Catalog.SATURATION_THRESHOLD)
                    .OrderBy(w => w.Total)
                    .FirstOrDefault();

                return fallback.User?.FullName;
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
                return await _userService.GetActiveUsersAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener usuarios activos desde la API externa.", ex);
            }

        }
        /// <summary>
        /// Método que obtiene un listado de elementos de trabajo aplicando filtros opcionales.
        /// Este método no aplica paginación y devuelve todos los elementos que cumplen con los criterios de filtrado.
        /// Devuelve lista de elementos de trabajo con fecha de vencimiento próxima a vencer, relevancia alta
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<List<WorkItemResponseDTO>> ListWork(WorkItemFilterDTO filter)
        {
            return await _workItemRepository.ListWork(filter);
        }
    }
}