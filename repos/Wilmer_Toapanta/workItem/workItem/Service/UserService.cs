using workItem.DTO;
using workItem.Service.IService;
using workItem.Shared;

namespace workItem.Service
{
    public class UserService(IConfiguration configuration): IUserService
    {
        private readonly string _userApiBaseUrl = configuration.GetValue<string>("urlUser")!;
        /// <summary>
        /// Obtiene la lista de usuarios activos llamando a la API externa de usuarios.
        /// Realiza una solicitud POST con los parámetros de filtrado para obtener usuarios con estado activo.
        /// </summary>
        /// <returns>Lista de usuarios activos desde la API externa</returns>
        /// <exception cref="HttpRequestException">Se lanza si la solicitud a la API externa falla</exception>
        public async Task<List<UserExternalDTO>> GetActiveUsersAsync()
        {
            try
            {
                using var client = new HttpClient();
                client.BaseAddress = new Uri(_userApiBaseUrl);
                // Construir el cuerpo de la solicitud para filtrar usuarios activos
                var body = new
                {
                    status = 1  // 1 = activo
                };
                // Realizar la solicitud POST a la API de usuarios
                var response = await client.PostAsJsonAsync("api/User/ListAllUsers", body);
                response.EnsureSuccessStatusCode();
                // Deserializar la respuesta o retornar lista vacía si no hay resultado
                var result = await response.Content.ReadFromJsonAsync<APIResponseDTO<List<UserExternalDTO>>>();
                return result?.Result ?? new List<UserExternalDTO>();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener usuarios activos desde la API externa.", ex);
            }

        }
    }
}
