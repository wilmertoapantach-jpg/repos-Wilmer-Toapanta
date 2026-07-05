using AutoMapper;
using project_user.DTO;
using project_user.Models;

namespace project_user.Shared
{
    /// <summary>
    /// Configuración de mapeo de AutoMapper para las entidades y DTOs de usuario.
    /// </summary>
    public class MappingConfig : Profile
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="MappingConfig"/> y define las reglas de mapeo.
        /// </summary>
        public MappingConfig()
        {
            // Mapeo bidireccional entre la entidad UserData y los DTOs de entrada y salida
            CreateMap<UserData, UserResponseDTO>().ReverseMap();
            CreateMap<UserData, UserRequestDTO>().ReverseMap();
        }
    }
}
