using workItem.DTO;
using workItem.Shared;

namespace workItem.Service.IService
{
    /// <summary>
    /// Interfaz del servicio para la lógica de negocio de elementos de trabajo.
    /// Define los métodos para guardar, listar y asignar elementos de trabajo con reglas de distribución.
    /// </summary>
    public interface IWorkItemService
    {
        /// <summary>Crea un nuevo elemento de trabajo o actualiza uno existente, aplicando reglas de asignación automática.</summary>
        Task<WorkItemResponseDTO> SaveWorkItem(WorkItemRequestDTO request);
        
        /// <summary>Obtiene un listado de elementos de trabajo con filtros opcionales y paginación.</summary>
        Task<PageResponseDTO<WorkItemResponseDTO>> ListWorkItems(WorkItemFilterDTO filter);
        
        /// <summary>Asigna o reasigna un elemento de trabajo existente al usuario más apropiado usando las reglas de distribución.</summary>
        Task<WorkItemResponseDTO> AssignWorkItem(int workItemId);
        /// <summary>
        /// Recupera una lista de elementos de trabajo según los filtros proporcionados, sin paginación.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<List<WorkItemResponseDTO>> ListWork(WorkItemFilterDTO filter);
    }
}
