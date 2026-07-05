using project_user.DTO;
using project_user.Shared;

namespace project_user.Service.IService
{
    /// <summary>
    /// Interfaz que define las operaciones de negocio para la gestión de usuarios.
    /// Contiene la lógica para el guardado (creación/actualización) y listado filtrado/paginado de usuarios.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Guarda un usuario en el sistema. Determina automáticamente si se trata de una creación
        /// o una actualización en función de la presencia y valor del ID del usuario.
        /// </summary>
        /// <param name="request">DTO con los datos del usuario a guardar.</param>
        /// <returns>DTO con la información del usuario creado o actualizado.</returns>
        Task<UserResponseDTO> SaveUser(UserRequestDTO request);

        /// <summary>
        /// Registra un nuevo usuario en el sistema. Realiza validaciones previas de campos obligatorios
        /// y duplicados de número de identificación entre usuarios activos.
        /// </summary>
        /// <param name="request">DTO con datos del nuevo usuario (ID nulo o 0).</param>
        /// <returns>DTO con el usuario recién creado, incluyendo el ID asignado.</returns>
        Task<UserResponseDTO> NewUser(UserRequestDTO request);

        /// <summary>
        /// Actualiza los datos de un usuario existente. Valida que el usuario exista, esté activo
        /// y que su número de identificación no cause conflictos con otros usuarios activos.
        /// </summary>
        /// <param name="request">DTO con los datos actualizados (ID mayor a 0).</param>
        /// <returns>DTO con los datos actualizados del usuario.</returns>
        Task<UserResponseDTO> UpdateUser(UserRequestDTO request);

        /// <summary>
        /// Obtiene un listado paginado y filtrado de usuarios.
        /// </summary>
        /// <param name="request">DTO que contiene filtros opcionales de búsqueda y parámetros de paginación.</param>
        /// <returns>Un objeto de respuesta paginada que contiene los usuarios encontrados.</returns>
        Task<PageResponseDTO<UserResponseDTO>> ListUsers(UserFilterDTO request);

        /// <summary>
        /// Obtiene un listado completo (sin paginación) y filtrado de usuarios.
        /// </summary>
        /// <param name="request">DTO que contiene filtros opcionales de búsqueda.</param>
        /// <returns>Lista de usuarios encontrados.</returns>
        Task<List<UserResponseDTO>> ListAllUsers(UserAllFilterDTO request);
    }
}
