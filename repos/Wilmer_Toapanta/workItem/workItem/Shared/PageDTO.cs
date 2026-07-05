namespace workItem.Shared
{
    /// <summary>
    /// DTO base para solicitudes paginadas.
    /// Define los parámetros de paginación (número de página y tamaño de página).
    /// </summary>
    public class PageRequestDTO
    {
        /// <summary>Número de la página a solicitar (por defecto 1, primera página)</summary>
        public int PageNumber { get; set; } = 1;
        
        /// <summary>Número de elementos por página (por defecto 10)</summary>
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// DTO genérico de respuesta paginada.
    /// Contiene una lista de elementos paginados junto con información de paginación.
    /// </summary>
    /// <typeparam name="T">Tipo de elementos en la página</typeparam>
    public class PageResponseDTO<T>
    {
        /// <summary>Lista de elementos en la página actual</summary>
        public List<T> Items { get; set; } = new();
        
        /// <summary>Total de elementos (sin considerar paginación)</summary>
        public int Count { get; set; }
        
        /// <summary>Número de la página actual</summary>
        public int PageNumber { get; set; }
        
        /// <summary>Número de elementos por página</summary>
        public int PageSize { get; set; }
    }
}
