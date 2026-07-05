using project_user.DTO;
using project_user.Shared;

namespace project_user.Repository.IRepository
{
    /// <summary>
    /// Interfaz que define las operaciones de acceso a datos para la entidad UserData (Usuarios).
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Inserta un nuevo registro de usuario en la base de datos.
        /// </summary>
        /// <param name="request">DTO con los datos del usuario a crear.</param>
        /// <returns>DTO con los datos del usuario creado, incluyendo el ID de la base de datos.</returns>
        Task<UserResponseDTO> CreateUser(UserRequestDTO request);

        /// <summary>
        /// Actualiza los campos de un usuario existente en la base de datos.
        /// </summary>
        /// <param name="request">DTO con los datos del usuario a actualizar (debe incluir el ID).</param>
        /// <returns>DTO con los datos del usuario tras la actualización.</returns>
        Task<UserResponseDTO> UpdateUser(UserRequestDTO request);

        /// <summary>
        /// Verifica si existe algún usuario activo que coincida con los criterios definidos en el filtro.
        /// </summary>
        /// <param name="filter">DTO que contiene filtros como el ID y/o número de identificación.</param>
        /// <returns><c>true</c> si existe un registro que coincide con los criterios y sigue activo; de lo contrario, <c>false</c>.</returns>
        Task<bool> ExistsUser(UserFilterDTO filter);

        /// <summary>
        /// Consulta la base de datos de usuarios aplicando filtros dinámicos y paginación.
        /// </summary>
        /// <param name="request">DTO con parámetros de filtrado y paginación.</param>
        /// <returns>Un objeto de respuesta paginada con la lista de usuarios y el total de registros.</returns>
        Task<PageResponseDTO<UserResponseDTO>> ListUsers(UserFilterDTO request);

        /// <summary>
        /// Consulta la base de datos para obtener todos los usuarios sin paginación, aplicando filtros dinámicos.
        /// </summary>
        /// <param name="request">DTO con parámetros de filtrado.</param>
        /// <returns>Lista de usuarios que coinciden con los filtros.</returns>
        Task<List<UserResponseDTO>> ListAllUsers(UserAllFilterDTO request);
    }
}
