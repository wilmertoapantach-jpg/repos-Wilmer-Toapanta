namespace workItem.DTO
{
    /// <summary>
    /// DTO para solicitar la creación o actualización de un elemento de trabajo.
    /// Si WorkItemId es 0, se creará un nuevo elemento. Si es > 0, se actualizará uno existente.
    /// </summary>
    public class WorkItemRequestDTO
    {
        /// <summary>Identificador del elemento (0 para creación, > 0 para actualización)</summary>
        public int WorkItemId { get; set; }
        
        /// <summary>Título del elemento de trabajo (obligatorio)</summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>Descripción detallada del elemento (opcional)</summary>
        public string? Description { get; set; }
        
        /// <summary>Fecha de vencimiento del elemento</summary>
        public DateTime DueDate { get; set; }

        /// <summary>Prioridad del elemento (valor numérico)</summary>
        /// 0 = Baja, 1 =  Alta
        public int Priority { get; set; }
        
        /// <summary>Estado del elemento (1 = activo, 0 = inactivo, por defecto 1)</summary>
        public short Status { get; set; } = 1;
        /// <summary>Identificador del usuario a quien está asignado este elemento de trabajo</summary>
        public int? AssignedUserId { get; set; }
    }
}
