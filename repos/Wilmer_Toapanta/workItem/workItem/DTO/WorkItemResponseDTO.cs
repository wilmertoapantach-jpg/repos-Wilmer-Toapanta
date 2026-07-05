namespace workItem.DTO
{
    /// <summary>
    /// DTO de respuesta devuelto después de guardar o listar un elemento de trabajo.
    /// Contiene toda la información del elemento incluyendo detalles de asignación.
    /// </summary>
    public class WorkItemResponseDTO
    {
        /// <summary>Identificador del elemento de trabajo</summary>
        public int WorkItemId { get; set; }
        
        /// <summary>Título del elemento de trabajo</summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>Descripción del elemento</summary>
        public string? Description { get; set; }
        
        /// <summary>Fecha de vencimiento del elemento</summary>
        public DateTime DueDate { get; set; }

        /// <summary>Prioridad del elemento</summary>
        /// 0 = Baja, 1 = Alta
        public int Priority { get; set; }
        
        /// <summary>Estado del elemento (1 = activo, 0 = inactivo)</summary>
        public short Status { get; set; }
        
        /// <summary>Identificador del usuario asignado a este elemento</summary>
        public int? AssignedUserId { get; set; }
        /// <summary>Nombre del usuario asignado a este elemento</summary>
        public string? AssignedUserName { get; set; }
        /// <summary>Fecha y hora de creación del elemento</summary>
        public DateTime CreatedDate { get; set; }
    }
}
