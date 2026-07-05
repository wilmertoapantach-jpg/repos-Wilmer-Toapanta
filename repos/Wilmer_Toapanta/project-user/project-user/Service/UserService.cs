using project_user.DTO;
using project_user.Repository.IRepository;
using project_user.Service.IService;
using project_user.Shared;

namespace project_user.Service
{
    public class UserService(IUserRepository userRepository) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository;

        /// <summary>
        /// Determina si se debe crear o actualizar un usuario según el valor del campo <c>Id</c>.
        /// Si <c>Id</c> es nulo o igual a 0, delega en <see cref="NewUser"/>;
        /// si es mayor a 0, delega en <see cref="UpdateUser"/>.
        /// </summary>
        /// <param name="request">Objeto con los datos del usuario a guardar.</param>
        /// <returns>DTO con los datos del usuario creado o actualizado.</returns>
        /// <exception cref="Exception">Si el objeto <paramref name="request"/> es nulo.</exception>
        public async Task<UserResponseDTO> SaveUser(UserRequestDTO request)
        {
            if (request is null) throw new Exception("La información del usuario no puede ser nula.");
            if (!request.Id.HasValue || request.Id == 0) return await NewUser(request);
            return await UpdateUser(request);
        }

        /// <summary>
        /// Crea un nuevo usuario en la base de datos.
        /// Antes de crear, valida que los campos obligatorios estén completos y que no exista
        /// un usuario activo con el mismo número de identificación (Status != 0).
        /// </summary>
        /// <param name="request">Objeto con los datos del nuevo usuario. El campo <c>Id</c> debe ser 0 o nulo.</param>
        /// <returns>DTO con los datos del usuario recién creado, incluyendo el <c>Id</c> generado por la base de datos.</returns>
        /// <exception cref="Exception">
        /// Si el request es nulo, si algún campo requerido (<c>IdentificationNumber</c>, <c>FullName</c>, <c>Email</c>) está vacío,
        /// o si ya existe un usuario activo con el mismo número de identificación.
        /// </exception>
        public async Task<UserResponseDTO> NewUser(UserRequestDTO request)
        {
            if (request is null) throw new Exception("La información del usuario no puede ser nula.");
            if (string.IsNullOrWhiteSpace(request.IdentificationNumber))
                throw new Exception("El número de identificación no puede ser nulo o vacío.");
            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new Exception("El nombre completo no puede ser nulo o vacío.");
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new Exception("El correo electrónico no puede ser nulo o vacío.");

            // Verificar si ya existe un usuario activo con el mismo número de identificación
            var exists = await _userRepository.ExistsUser(new UserFilterDTO
            {
                IdentificationNumber = request.IdentificationNumber
            });

            if (exists)
                throw new Exception("Ya existe un usuario activo registrado con ese número de identificación.");

            return await _userRepository.CreateUser(request);
        }

        /// <summary>
        /// Actualiza los datos de un usuario existente en la base de datos.
        /// Valida que el usuario a modificar exista y esté activo, y que el número de identificación
        /// no pertenezca ya a otro usuario activo diferente.
        /// </summary>
        /// <param name="request">Objeto con los datos actualizados del usuario. El campo <c>Id</c> debe ser mayor a 0.</param>
        /// <returns>DTO con los datos del usuario después de la actualización.</returns>
        /// <exception cref="Exception">
        /// Si el request es nulo, si <c>Id</c> es 0 o nulo, si algún campo requerido está vacío,
        /// si el usuario no existe o está eliminado, o si el número de identificación ya pertenece a otro usuario activo.
        /// </exception>
        public async Task<UserResponseDTO> UpdateUser(UserRequestDTO request)
        {
            if (request is null) throw new Exception("La información del usuario no puede ser nula.");
            if (!request.Id.HasValue || request.Id == 0)
                throw new Exception("El ID del usuario no puede ser cero para una actualización.");
            if (string.IsNullOrWhiteSpace(request.IdentificationNumber))
                throw new Exception("El número de identificación no puede ser nulo o vacío.");
            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new Exception("El nombre completo no puede ser nulo o vacío.");
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new Exception("El correo electrónico no puede ser nulo o vacío.");

            // Verificar que el usuario a actualizar exista y esté activo
            var userExists = await _userRepository.ExistsUser(new UserFilterDTO
            {
                Id = request.Id
            });

            if (!userExists)
                throw new Exception("El usuario indicado no existe o ha sido eliminado.");

            // Verificar que el número de identificación no pertenezca a otro usuario activo distinto
            var idNumberBelongsToAnother = await _userRepository.ExistsUser(new UserFilterDTO
            {
                IdentificationNumber = request.IdentificationNumber
            });

            if (idNumberBelongsToAnother)
            {
                // Comprobar si el número de identificación corresponde al mismo usuario que se está actualizando
                var sameUser = await _userRepository.ExistsUser(new UserFilterDTO
                {
                    Id = request.Id,
                    IdentificationNumber = request.IdentificationNumber
                });

                if (!sameUser)
                    throw new Exception("El número de identificación ya pertenece a otro usuario activo.");
            }

            return await _userRepository.UpdateUser(request);
        }

        /// <summary>
        /// Obtiene una lista paginada de usuarios aplicando los filtros opcionales del request.
        /// Si no se especifica el estado (<c>Status</c>), por defecto solo retorna usuarios activos (Status != 0).
        /// </summary>
        /// <param name="request">
        /// Objeto con filtros opcionales (<c>IdentificationNumber</c>, <c>FullName</c>, <c>Status</c>)
        /// y parámetros de paginación (<c>PageNumber</c>, <c>PageSize</c>).
        /// </param>
        /// <returns>
        /// Objeto <see cref="PageResponseDTO{T}"/> que contiene:
        /// <list type="bullet">
        ///   <item><description><c>Items</c>: lista de usuarios encontrados.</description></item>
        ///   <item><description><c>Count</c>: total de registros que coinciden con los filtros.</description></item>
        ///   <item><description><c>PageNumber</c> y <c>PageSize</c>: página y tamaño de página aplicados.</description></item>
        /// </list>
        /// </returns>
        public async Task<PageResponseDTO<UserResponseDTO>> ListUsers(UserFilterDTO request)
        {
            return await _userRepository.ListUsers(request);
        }

        /// <summary>
        /// Obtiene un listado completo (sin paginación) de usuarios aplicando los filtros opcionales del request.
        /// </summary>
        /// <param name="request">Objeto con filtros opcionales.</param>
        /// <returns>Lista de usuarios encontrados.</returns>
        public async Task<List<UserResponseDTO>> ListAllUsers(UserAllFilterDTO request)
        {
            return await _userRepository.ListAllUsers(request);
        }
    }
}
