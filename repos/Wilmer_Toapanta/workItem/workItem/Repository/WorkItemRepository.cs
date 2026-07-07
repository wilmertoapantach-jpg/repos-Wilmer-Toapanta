using AutoMapper;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using workItem.Data;
using workItem.DTO;
using workItem.Models;
using workItem.Repository.IRepository;
using workItem.Shared;

namespace workItem.Repository
{
    public class WorkItemRepository(IDbContextFactory<workManagementContext> contextFactory, IMapper mapper) : IWorkItemRepository
    {
        private readonly IDbContextFactory<workManagementContext> _contextFactory = contextFactory;
        private readonly IMapper _mapper = mapper;


        /// <summary>
        /// Crea un nuevo elemento de trabajo en la base de datos.
        /// Asigna automáticamente la fecha de creación a la fecha/hora actual en UTC.
        /// </summary>
        /// <param name="request">DTO con los datos del elemento de trabajo a crear</param>
        /// <returns>DTO de respuesta con el elemento de trabajo creado</returns>
        public async Task<WorkItemResponseDTO> CreateWorkItem(WorkItemRequestDTO request)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = _mapper.Map<WorkItem>(request);
            entity.AssignedUserName = request.AssignedUserName;
            entity.CreatedDate = DateTime.UtcNow;
            context.WorkItem.Add(entity);
            await context.SaveChangesAsync();
            return _mapper.Map<WorkItemResponseDTO>(entity);
        }

        /// <summary>
        /// Actualiza un elemento de trabajo existente en la base de datos.
        /// Modifica los campos: título, descripción, fecha de vencimiento, prioridad, relevancia y estado.
        /// </summary>
        /// <param name="request">DTO con los datos actualizados del elemento de trabajo</param>
        /// <returns>DTO de respuesta con el elemento de trabajo actualizado</returns>
        /// <exception cref="Exception">Se lanza si el elemento de trabajo no existe</exception>
        public async Task<WorkItemResponseDTO> UpdateWorkItem(WorkItemRequestDTO request)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.WorkItem.FindAsync(request.WorkItemId)
                ?? throw new Exception($"WorkItem con Id {request.WorkItemId} no encontrado.");
            entity.Title = request.Title;
            entity.Description = request.Description;
            entity.DueDate = request.DueDate;
            entity.Priority = request.Priority;
            entity.Status = request.Status;
            context.WorkItem.Update(entity);
            await context.SaveChangesAsync();
            return _mapper.Map<WorkItemResponseDTO>(entity);
        }

        /// <summary>
        /// Obtiene un listado de elementos de trabajo con filtros opcionales y paginación.
        /// Los filtros disponibles son: ID, estado, relevancia e ID del usuario asignado.
        /// </summary>
        /// <param name="filter">Objeto con criterios de filtrado y parámetros de paginación</param>
        /// <returns>Página con los elementos de trabajo filtrados</returns>
        public async Task<PageResponseDTO<WorkItemResponseDTO>> ListWorkItems(WorkItemFilterDTO filter)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var predicate = PredicateBuilder.New<WorkItem>(true);

            if (filter.WorkItemId.HasValue && filter.WorkItemId > 0) predicate = predicate.And(w => w.Id == filter.WorkItemId.Value);
            if (filter.Status.HasValue) predicate = predicate.And(w => w.Status == filter.Status.Value);
            if (!string.IsNullOrEmpty(filter.AssignedUserName)) predicate = predicate.And(w => w.AssignedUserName == filter.AssignedUserName);
            if (filter.Relevance.HasValue) predicate = predicate.And(w => w.Priority == filter.Relevance.Value);

            var count = await context.WorkItem.CountAsync(predicate);
            var pageNumber = filter.PageNumber > 0 ? filter.PageNumber : 1;
            var pageSize = filter.PageSize > 0 ? filter.PageSize : 10;

            var data = await context.WorkItem
                .AsNoTracking()
                .Where(predicate)
                // 1. Primero las fechas más próximas a vencer (Ascendente: fechas menores primero)
                .OrderBy(w => w.DueDate)
                // 2. Luego, si tienen la misma fecha, los Urgentes primero (Descendente: 1 antes que 0)
                .ThenByDescending(w => w.Priority)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            //Obtenemos los nombres únicos de usuarios que salieron en el resultado para calcular sus métricas de golpe.
            var distinctUsers = data.Select(w => w.AssignedUserName).Where(u => !string.IsNullOrEmpty(u)).Distinct().ToList();
            if (distinctUsers.Count > 0) predicate = predicate.And(w => distinctUsers.Contains(w.AssignedUserName));

            //Agrupamos y contamos en una SOLA consulta rápida a la base de datos.
            var userMetrics = await context.WorkItem
                .Where(predicate)
                .GroupBy(x => x.AssignedUserName)
                .Select(g => new
                {
                    UserName = g.Key,
                    Total = g.Count(),
                    High = g.Count(x => x.Priority == Catalog.HIGH_RELEVANCE),
                    Low = g.Count(x => x.Priority == Catalog.LOW_RELEVANCE)
                })
                .ToDictionaryAsync(x => x.UserName, x => x);

            //Mapeamos en memoria uniendo la info de la tarea con las métricas calculadas.
            var result = data.Select(w =>
            {
                //Declaramos la variable fuera para evitar el error de ámbito de la expresión lambda
                var total = 0;
                var high = 0;
                var low = 0;

                //Evaluamos de forma segura si el usuario tiene métricas en el diccionario
                if (!string.IsNullOrEmpty(w.AssignedUserName) && userMetrics.TryGetValue(w.AssignedUserName, out var metrics))
                {
                    total = metrics.Total;
                    high = metrics.High;
                    low = metrics.Low;
                }

                //Retornamos el DTO ya mapeado con las variables locales limpias
                return new WorkItemResponseDTO
                {
                    WorkItemId = w.Id,
                    Title = w.Title,
                    Description = w.Description,
                    DueDate = w.DueDate,
                    Priority = w.Priority,
                    Status = w.Status,
                    AssignedUserName = w.AssignedUserName,
                    CreatedDate = w.CreatedDate,
                    Total = total,
                    High = high,
                    Low = low
                };
            }).ToList();
            // Retornamos la respuesta paginada con los elementos de trabajo y las métricas calculadas.
            return new PageResponseDTO<WorkItemResponseDTO>
            {
                Items = result,
                Count = count,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Verifica si existe un elemento de trabajo con el ID especificado.
        /// </summary>
        /// <param name="workItemId">ID del elemento de trabajo a verificar</param>
        /// <returns>Verdadero si existe, falso en caso contrario</returns>
        public async Task<bool> ExistsWorkItem(int workItemId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.WorkItem.AnyAsync(w => w.Id == workItemId);
        }

        /// <summary>
        /// Obtiene todos los elementos de trabajo activos asignados a un usuario.
        /// Se utiliza en la lógica de asignación para calcular la carga de trabajo del usuario.
        /// </summary>
        /// <param name="username">ID externo del usuario como cadena de texto</param>
        /// <returns>Lista de elementos de trabajo activos asignados al usuario</returns>
        public async Task<List<WorkItemResponseDTO>> GetWorkItemsByUser(List<string> username)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var data = await context.WorkItem
                    .AsNoTracking()
                    .Where(w => username.Contains(w.AssignedUserName) && w.Status == 1)
                    .ToListAsync();
                return _mapper.Map<List<WorkItemResponseDTO>>(data);
            }
            catch (Exception ex)
            {
                // Manejo de la excepción, por ejemplo, registrando el error
                Console.WriteLine($"Error al obtener los elementos de trabajo: {ex.Message}");
                throw; // Re-lanzar la excepción si es necesario
            }

        }

        /// <summary>
        /// Cuenta los elementos de trabajo activos con relevancia alta asignados a un usuario.
        /// Se utiliza para verificar si el usuario está saturado con tareas de alta importancia.
        /// </summary>
        /// <param name="username">ID externo del usuario como cadena de texto</param>
        /// <returns>Número de elementos activos con relevancia alta asignados al usuario</returns>
        public async Task<List<WorkItemResponseDTO>> CountHighRelevanceByUser(List<string> usernames)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.WorkItem
           .AsNoTracking()
           .Where(w => w.Status == Catalog.STATUS_ACTIVE
                       && w.Priority == Catalog.HIGH_RELEVANCE
                       && usernames.Contains(w.AssignedUserName.ToLower().Trim()))
           .GroupBy(w => w.AssignedUserName)
           .Where(g => g.Count() >= 3)
           .Select(g => new WorkItemResponseDTO
           {
               AssignedUserName = g.Key,
               High = g.Count()
           })
           .ToListAsync();
        }

        /// <summary>
        /// Método para listar elementos de trabajo con filtros y ordenamiento específicos.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>

        public async Task<List<WorkItemResponseDTO>> ListWork(WorkItemFilterDTO filter)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var predicate = PredicateBuilder.New<WorkItem>(true);

            if (filter.WorkItemId.HasValue && filter.WorkItemId > 0) predicate = predicate.And(w => w.Id == filter.WorkItemId.Value);
            if (filter.Status.HasValue) predicate = predicate.And(w => w.Status == filter.Status.Value);
            if (!string.IsNullOrEmpty(filter.AssignedUserName)) predicate = predicate.And(w => w.AssignedUserName.ToLower().Trim() == filter.AssignedUserName.ToLower().Trim());

            // 1. Traemos la data necesaria aplicando los filtros y ordenamiento primero.
            var dbData = await context.WorkItem
                .Where(predicate)
                .OrderBy(w => w.DueDate)
                .ThenByDescending(w => w.Priority)
                .Select(w => new
                {
                    w.Id,
                    w.Title,
                    w.Description,
                    w.DueDate,
                    w.Priority,
                    w.Status,
                    w.AssignedUserName,
                    w.CreatedDate
                })
                .ToListAsync();

            if (!dbData.Any()) return new List<WorkItemResponseDTO>();

            // 2. Obtenemos los nombres únicos de usuarios que salieron en el resultado para calcular sus métricas de golpe.
            var distinctUsers = dbData.Select(w => w.AssignedUserName).Where(u => !string.IsNullOrEmpty(u)).Distinct().ToList();

            // 3. Agrupamos y contamos en una SOLA consulta rápida a la base de datos.
            var userMetrics = await context.WorkItem
                .Where(x => distinctUsers.Contains(x.AssignedUserName) && x.Status == 1)
                .GroupBy(x => x.AssignedUserName)
                .Select(g => new
                {
                    UserName = g.Key,
                    Total = g.Count(),
                    High = g.Count(x => x.Priority == Catalog.HIGH_RELEVANCE),
                    Low = g.Count(x => x.Priority == Catalog.LOW_RELEVANCE)
                })
                .ToDictionaryAsync(x => x.UserName, x => x);

            // 4. Mapeamos en memoria uniendo la info de la tarea con las métricas calculadas.
            var result = dbData.Select(w =>
            {
                // 1. Declaramos la variable fuera para evitar el error de ámbito de la expresión lambda
                var total = 0;
                var high = 0;
                var low = 0;

                // 2. Evaluamos de forma segura si el usuario tiene métricas en el diccionario
                if (!string.IsNullOrEmpty(w.AssignedUserName) && userMetrics.TryGetValue(w.AssignedUserName, out var metrics))
                {
                    total = metrics.Total;
                    high = metrics.High;
                    low = metrics.Low;
                }

                // 3. Retornamos el DTO ya mapeado con las variables locales limpias
                return new WorkItemResponseDTO
                {
                    WorkItemId = w.Id,
                    Title = w.Title,
                    Description = w.Description,
                    DueDate = w.DueDate,
                    Priority = w.Priority,
                    Status = w.Status,
                    AssignedUserName = w.AssignedUserName,
                    CreatedDate = w.CreatedDate,
                    Total = total,
                    High = high,
                    Low = low
                };
            }).ToList();

            return result;
        }
    }
}
