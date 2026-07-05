using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
            entity.AssignedUserId = request.AssignedUserId;
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
            var query = context.WorkItem.AsNoTracking().AsQueryable();

            if (filter.WorkItemId.HasValue && filter.WorkItemId > 0)
                query = query.Where(w => w.Id == filter.WorkItemId.Value);
            if (filter.Status.HasValue)
                query = query.Where(w => w.Status == filter.Status.Value);

            if (filter.AssignedUserId.HasValue)
                query = query.Where(w => w.AssignedUserId == filter.AssignedUserId.Value);

            var count = await query.CountAsync();
            var pageNumber = filter.PageNumber > 0 ? filter.PageNumber : 1;
            var pageSize = filter.PageSize > 0 ? filter.PageSize : 10;

            var data = await query
                .OrderBy(w => w.DueDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PageResponseDTO<WorkItemResponseDTO>
            {
                Items = _mapper.Map<List<WorkItemResponseDTO>>(data),
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
        public async Task<List<WorkItemResponseDTO>> GetWorkItemsByUser(int userId)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var data = await context.WorkItem
                    .AsNoTracking()
                    .Where(w => w.AssignedUserId == userId && w.Status == 1)
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
        public async Task<int> CountHighRelevanceByUser(string username)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            if (!int.TryParse(username, out var userId)) return 0;
            return await context.WorkItem
                .AsNoTracking()
                .Where(w => w.AssignedUserId == userId && w.Status == 1
                            && w.Priority == 1)
                .CountAsync();
        }
    }
}
