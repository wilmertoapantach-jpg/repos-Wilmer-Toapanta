namespace workItem.DTO
{
    public class UserExternalDTO
    {
        /// <summary>Identificador único del usuario en el sistema externo</summary>
        public int Id { get; set; }

        /// <summary>Número de identificación del usuario (cédula, pasaporte, etc.)</summary>
        public string? IdentificationNumber { get; set; }

        /// <summary>Nombre completo del usuario</summary>
        public string? FullName { get; set; }

        /// <summary>Estado del usuario (1 = activo, 0 = inactivo)</summary>
        public int Status { get; set; }
    }
}
