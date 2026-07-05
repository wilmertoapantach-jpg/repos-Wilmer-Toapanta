using project_user.Shared;

namespace project_user.DTO
{
    public class UserFilterDTO : PageRequestDTO
    {
        /// <summary>
        /// Filtrar por ID del usuario (opcional, usado internamente para validaciones de existencia).
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Filtrar por número de identificación (opcional).
        /// </summary>
        public string? IdentificationNumber { get; set; }

        /// <summary>
        /// Filtrar por nombre completo (opcional).
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// Filtrar por estado (opcional).
        /// 0 = Eliminado, 1 = Activo
        /// Si no se especifica, se devuelven solo los activos (Status != 0).
        /// </summary>
        public short? Status { get; set; }
    }
}
