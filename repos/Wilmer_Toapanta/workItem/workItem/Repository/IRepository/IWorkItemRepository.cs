using workItem.DTO;
using workItem.Shared;

namespace workItem.Repository.IRepository
{
    /// <summary>
    /// Interfaz del repositorio para operaciones CRUD de elementos de trabajo.
    /// Define los métodos para crear, actualizar, listar y consultar elementos de trabajo.
    /// </summary>
    public interface IWorkItemRepository
    {
        /// <summary>Crea un nuevo elemento de trabajo en la base de datos.</summary>
        Task<WorkItemResponseDTO> CreateWorkItem(WorkItemRequestDTO request);
        
        /// <summary>Actualiza un elemento de trabajo existente en la base de datos.</summary>
        Task<WorkItemResponseDTO> UpdateWorkItem(WorkItemRequestDTO request);
        
        /// <summary>Obtiene un listado de elementos de trabajo con filtros opcionales y paginación.</summary>
        Task<PageResponseDTO<WorkItemResponseDTO>> ListWorkItems(WorkItemFilterDTO filter);
        
        /// <summary>Verifica si existe un elemento de trabajo con el ID especificado.</summary>
        Task<bool> ExistsWorkItem(int workItemId);
        
        /// <summary>Obtiene todos los elementos de trabajo activos asignados a un usuario para la lógica de asignación.</summary>
        Task<List<WorkItemResponseDTO>> GetWorkItemsByUser(int userId);
        
        /// <summary>Cuenta los elementos de trabajo activos con relevancia alta asignados a un usuario para verificar saturación.</summary>
        Task<int> CountHighRelevanceByUser(string username);
    }
}
