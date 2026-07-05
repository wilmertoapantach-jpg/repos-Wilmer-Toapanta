using workItem.Shared;

namespace workItem.DTO
{
    /// <summary>
    /// DTO de filtrado utilizado para obtener un listado de elementos de trabajo con filtros opcionales y paginación.
    /// Hereda de PageRequestDTO para incluir los parámetros de paginación.
    /// </summary>
    public class WorkItemFilterDTO : PageRequestDTO
    {
        /// <summary>Filtro opcional: ID del elemento de trabajo (si se especifica, retorna solo ese elemento)</summary>
        public int? WorkItemId { get; set; }
        
        /// <summary>Filtro opcional: estado del elemento (1 = activo, 0 = inactivo)</summary>
        public short? Status { get; set; }
        
        /// <summary>Filtro opcional: relevancia del elemento ("High" o "Low")</summary>
        public string? Relevance { get; set; }
        
        /// <summary>Filtro opcional: ID del usuario asignado (retorna elementos asignados a ese usuario)</summary>
        public int? AssignedUserId { get; set; }
    }
}
