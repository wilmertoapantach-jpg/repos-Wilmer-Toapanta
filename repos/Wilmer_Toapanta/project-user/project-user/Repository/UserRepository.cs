using AutoMapper;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using project_user.Data;
using project_user.DTO;
using project_user.Models;
using project_user.Repository.IRepository;
using project_user.Shared;

namespace project_user.Repository
{
    public class UserRepository(IDbContextFactory<userManagementContext> contextFactory, IMapper mapper) : IUserRepository
    {
        private readonly IDbContextFactory<userManagementContext> _contextFactory = contextFactory;
        private readonly IMapper _mapper = mapper;

        /// <summary>
        /// Verifica si existe al menos un usuario activo (Status != 0) que coincida con los filtros proporcionados.
        /// Se utiliza para validar duplicados antes de crear o actualizar un usuario.
        /// </summary>
        /// <param name="filter">
        /// Objeto de filtros. Se puede filtrar por <c>Id</c>, <c>IdentificationNumber</c> o ambos.
        /// Solo se consideran usuarios con Status distinto de 0 (no eliminados).
        /// </param>
        /// <returns>
        /// <c>true</c> si existe al menos un usuario activo que coincida con los filtros;
        /// <c>false</c> si no existe ninguno.
        /// </returns>
        public async Task<bool> ExistsUser(UserFilterDTO filter)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var predicate = PredicateBuilder.New<UserData>(true);

                // Solo considera usuarios activos (no eliminados)
                predicate = predicate.And(u => u.Status != 0);

                if (!string.IsNullOrWhiteSpace(filter.IdentificationNumber))
                    predicate = predicate.And(u => u.IdentificationNumber == filter.IdentificationNumber);

                if (filter.Id.HasValue && filter.Id > 0)
                    predicate = predicate.And(u => u.Id == filter.Id.Value);

                return await context.UserData.AnyAsync(predicate);
            }
            catch (Exception ex)
            {
                // Manejo de excepciones (logging, rethrow, etc.)
                throw new Exception("Error al verificar la existencia del usuario", ex);
            }

        }

        /// <summary>
        /// Crea un nuevo registro de usuario en la base de datos.
        /// El estado se establece automáticamente en 1 (Activo) al momento de la creación.
        /// </summary>
        /// <param name="request">Objeto con los datos del usuario a crear (sin <c>Id</c>, lo genera la BD).</param>
        /// <returns>DTO con los datos del usuario recién creado, incluyendo el <c>Id</c> generado.</returns>
        public async Task<UserResponseDTO> CreateUser(UserRequestDTO request)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var entity = _mapper.Map<UserData>(request);
                entity.Status = 1; // Activo por defecto al crear

                context.UserData.Add(entity);
                await context.SaveChangesAsync();

                return _mapper.Map<UserResponseDTO>(entity);
            }
            catch (Exception ex)
            {
                // Manejo de excepciones (logging, rethrow, etc.)
                throw new Exception("Error al crear el usuario", ex);
            }

        }

        /// <summary>
        /// Actualiza un registro de usuario existente en la base de datos.
        /// Se mapean todos los campos del request al entity y se persisten los cambios.
        /// </summary>
        /// <param name="request">Objeto con los datos actualizados del usuario. El campo <c>Id</c> debe ser mayor a 0.</param>
        /// <returns>DTO con los datos del usuario después de la actualización.</returns>
        public async Task<UserResponseDTO> UpdateUser(UserRequestDTO request)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var entity = _mapper.Map<UserData>(request);

                context.UserData.Update(entity);
                await context.SaveChangesAsync();

                return _mapper.Map<UserResponseDTO>(entity);
            }
            catch (Exception ex)
            {
                // Manejo de excepciones (logging, rethrow, etc.)
                throw new Exception("Error al actualizar el usuario", ex);
            }

        }

        /// <summary>
        /// Obtiene una lista paginada de usuarios aplicando filtros dinámicos construidos con LinqKit.
        /// Si no se especifica el estado (<c>Status</c>), por defecto solo retorna usuarios activos (Status != 0).
        /// </summary>
        /// <param name="request">
        /// Objeto con filtros opcionales (<c>IdentificationNumber</c>, <c>FullName</c>, <c>Status</c>)
        /// y parámetros de paginación (<c>PageNumber</c>, <c>PageSize</c>).
        /// </param>
        /// <returns>
        /// Objeto <see cref="PageResponseDTO{T}"/> que contiene:
        /// <list type="bullet">
        ///   <item><description><c>Items</c>: lista de usuarios encontrados en la página solicitada.</description></item>
        ///   <item><description><c>Count</c>: total de registros que coinciden con los filtros (sin paginar).</description></item>
        ///   <item><description><c>PageNumber</c>: número de página aplicado (mínimo 1).</description></item>
        ///   <item><description><c>PageSize</c>: tamaño de página aplicado (mínimo 10 por defecto).</description></item>
        /// </list>
        /// </returns>
        public async Task<PageResponseDTO<UserResponseDTO>> ListUsers(UserFilterDTO request)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var predicate = PredicateBuilder.New<UserData>(true);

                // Filtrar por estado: si no se especifica, solo retorna activos
                if (request.Status.HasValue)
                    predicate = predicate.And(u => u.Status == request.Status.Value);
                else
                    predicate = predicate.And(u => u.Status != 0);

                if (!string.IsNullOrWhiteSpace(request.IdentificationNumber))
                    predicate = predicate.And(u => u.IdentificationNumber == request.IdentificationNumber);

                if (!string.IsNullOrWhiteSpace(request.FullName))
                    predicate = predicate.And(u => u.FullName.Contains(request.FullName));

                var count = await context.UserData.AsNoTracking().Where(predicate).CountAsync();

                var pageNumber = request.PageNumber > 0 ? request.PageNumber : 1;
                var pageSize = request.PageSize > 0 ? request.PageSize : 10;

                var data = await context.UserData
                    .AsNoTracking()
                    .Where(predicate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PageResponseDTO<UserResponseDTO>
                {
                    Items = _mapper.Map<List<UserResponseDTO>>(data),
                    Count = count,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                // Manejo de excepciones (logging, rethrow, etc.)
                throw new Exception("Error al listar los usuarios", ex);
            }

        }

        /// <summary>
        /// Consulta la base de datos para obtener todos los usuarios sin paginación, aplicando filtros dinámicos.
        /// </summary>
        /// <param name="request">DTO con parámetros de filtrado.</param>
        /// <returns>Lista de usuarios encontrados.</returns>
        public async Task<List<UserResponseDTO>> ListAllUsers(UserAllFilterDTO request)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var predicate = PredicateBuilder.New<UserData>(true);

                // Filtrar por estado: si no se especifica, solo retorna activos
                if (request.Status.HasValue)
                    predicate = predicate.And(u => u.Status == request.Status.Value);
                else
                    predicate = predicate.And(u => u.Status != 0);

                if (!string.IsNullOrWhiteSpace(request.IdentificationNumber))
                    predicate = predicate.And(u => u.IdentificationNumber == request.IdentificationNumber);

                if (!string.IsNullOrWhiteSpace(request.FullName))
                    predicate = predicate.And(u => u.FullName.Contains(request.FullName));

                var data = await context.UserData
                    .AsNoTracking()
                    .Where(predicate)
                    .ToListAsync();

                return _mapper.Map<List<UserResponseDTO>>(data);
            }
            catch (Exception ex)
            {
                // Manejo de excepciones (logging, rethrow, etc.)
                throw new Exception("Error al listar todos los usuarios", ex);
            }

        }
    }
}
