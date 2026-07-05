namespace project_user.Shared
{
    /// <summary>
    /// DTO base para realizar peticiones paginadas.
    /// </summary>
    public class PageRequestDTO
    {
        /// <summary>
        /// Número de página a consultar (inicia en 1).
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Cantidad de registros a retornar por página.
        /// </summary>
        public int PageSize { get; set; }
    }
}
