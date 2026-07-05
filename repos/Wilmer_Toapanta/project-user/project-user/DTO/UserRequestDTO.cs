namespace project_user.DTO
{
    public class UserRequestDTO
    {
        /// <summary>
        /// ID del usuario. Si es 0 o null se crea un nuevo registro; si es mayor a 0 se actualiza.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Número de identificación único del usuario.
        /// </summary>
        public string IdentificationNumber { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo del usuario.
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Estado del usuario:
        /// 0 = Eliminado
        /// 1 = Activo
        /// </summary>
        public short Status { get; set; }
    }
}
