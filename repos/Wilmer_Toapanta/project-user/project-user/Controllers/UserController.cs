using Microsoft.AspNetCore.Mvc;
using project_user.DTO;
using project_user.Service.IService;
using project_user.Shared;
using System.Net;

namespace project_user.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        /// <summary>
        /// Crea o actualiza un usuario según el valor del campo <c>Id</c>.
        /// <para>
        /// - Si <c>Id</c> es 0 o nulo: crea un nuevo usuario. Valida que no exista otro usuario activo
        ///   con el mismo número de identificación.
        /// </para>
        /// <para>
        /// - Si <c>Id</c> es mayor a 0: actualiza el usuario existente. Valida que el usuario esté activo
        ///   y que el número de identificación no pertenezca a otro usuario activo.
        /// </para>
        /// </summary>
        /// <param name="request">
        /// Objeto con los datos del usuario:
        /// <list type="bullet">
        ///   <item><description><c>Id</c>: 0 o null = crear; mayor a 0 = actualizar.</description></item>
        ///   <item><description><c>IdentificationNumber</c>: requerido, único entre usuarios activos.</description></item>
        ///   <item><description><c>FullName</c>: requerido.</description></item>
        ///   <item><description><c>Email</c>: requerido.</description></item>
        ///   <item><description><c>Status</c>: 0 = eliminado, 1 = activo.</description></item>
        /// </list>
        /// </param>
        /// <returns>
        /// <c>200 OK</c> con el DTO del usuario creado o actualizado dentro de <see cref="APIResponseDTO{T}"/>.
        /// <c>400 BadRequest</c> con el mensaje de error si alguna validación falla.
        /// </returns>
        [HttpPost("SaveUser")]
        public async Task<ActionResult<APIResponseDTO<UserResponseDTO>>> SaveUser([FromBody] UserRequestDTO request)
        {
            try
            {
                var result = await _userService.SaveUser(request);
                return Ok(new APIResponseDTO<UserResponseDTO>
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponseDTO<UserResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Messages = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Retorna una lista paginada de usuarios con filtros opcionales.
        /// Por defecto, solo incluye usuarios activos (Status != 0) si no se especifica el estado.
        /// </summary>
        /// <param name="request">
        /// Objeto con parámetros de búsqueda y paginación:
        /// <list type="bullet">
        ///   <item><description><c>IdentificationNumber</c>: filtrar por número de identificación exacto (opcional).</description></item>
        ///   <item><description><c>FullName</c>: filtrar por nombre completo parcial (opcional).</description></item>
        ///   <item><description><c>Status</c>: filtrar por estado (0 = eliminado, 1 = activo). Si se omite, retorna solo activos.</description></item>
        ///   <item><description><c>PageNumber</c>: número de página (por defecto 1).</description></item>
        ///   <item><description><c>PageSize</c>: registros por página (por defecto 10).</description></item>
        /// </list>
        /// </param>
        /// <returns>
        /// <c>200 OK</c> con un <see cref="PageResponseDTO{T}"/> que contiene:
        /// <c>Items</c> (lista de usuarios), <c>Count</c> (total), <c>PageNumber</c> y <c>PageSize</c>.
        /// <c>400 BadRequest</c> con el mensaje de error si ocurre algún problema.
        /// </returns>
        [HttpPost("ListUsers")]
        public async Task<ActionResult<APIResponseDTO<PageResponseDTO<UserResponseDTO>>>> ListUsers([FromBody] UserFilterDTO request)
        {
            try
            {
                var result = await _userService.ListUsers(request);
                return Ok(new APIResponseDTO<PageResponseDTO<UserResponseDTO>>
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponseDTO<PageResponseDTO<UserResponseDTO>>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Messages = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Retorna la lista completa de todos los usuarios sin paginación, aplicando filtros opcionales.
        /// Por defecto, solo incluye usuarios activos (Status != 0) si no se especifica el estado.
        /// </summary>
        /// <param name="request">Objeto con parámetros de búsqueda opcionales.</param>
        /// <returns>
        /// <c>200 OK</c> con una lista de usuarios dentro de <see cref="APIResponseDTO{T}"/>.
        /// <c>400 BadRequest</c> con el mensaje de error si ocurre algún problema.
        /// </returns>
        [HttpPost("ListAllUsers")]
        public async Task<ActionResult<APIResponseDTO<List<UserResponseDTO>>>> ListAllUsers([FromBody] UserAllFilterDTO request)
        {
            try
            {
                var result = await _userService.ListAllUsers(request);
                return Ok(new APIResponseDTO<List<UserResponseDTO>>
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponseDTO<List<UserResponseDTO>>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Messages = new List<string> { ex.Message }
                });
            }
        }
    }
}
