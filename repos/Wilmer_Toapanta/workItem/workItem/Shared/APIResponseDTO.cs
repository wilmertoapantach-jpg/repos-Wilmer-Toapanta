using System.Net;

namespace workItem.Shared
{
    /// <summary>
    /// DTO genérico de respuesta de API que encapsula la respuesta de cualquier endpoint.
    /// Proporciona información estandarizada sobre el éxito/fallo y los datos retornados.
    /// </summary>
    /// <typeparam name="T">Tipo de datos que se retorna en el resultado</typeparam>
    public class APIResponseDTO<T>
    {
        /// <summary>Código HTTP de estado de la respuesta</summary>
        public HttpStatusCode StatusCode { get; set; }
        
        /// <summary>Indica si la operación fue exitosa (true) o falló (false)</summary>
        public bool IsSuccess { get; set; }
        
        /// <summary>Lista de mensajes de error o información adicional (opcional)</summary>
        public List<string>? Messages { get; set; }
        
        /// <summary>Datos devueltos por la operación (null si hay error)</summary>
        public T? Result { get; set; }
    }
}
