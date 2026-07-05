namespace project_user.Shared
{
    /// <summary>
    /// Estructura estándar para envolver las respuestas paginadas del sistema.
    /// Contiene la sublista de elementos correspondientes a la página consultada y la metadata de paginación.
    /// </summary>
    /// <typeparam name="T">El tipo de los elementos contenidos en la lista paginada.</typeparam>
    public class PageResponseDTO<T>
    {
        /// <summary>
        /// Lista de elementos pertenecientes a la página actual.
        /// </summary>
        public List<T> Items { get; set; } = [];

        /// <summary>
        /// Total de elementos disponibles en la colección filtrada original (sin paginar).
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Cantidad de registros devueltos por página.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Número de página actual.
        /// </summary>
        public int PageNumber { get; set; }
    }
}
