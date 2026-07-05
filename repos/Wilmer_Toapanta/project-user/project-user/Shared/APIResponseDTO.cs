using System.Net;

namespace project_user.Shared
{
    /// <summary>
    /// Representa la estructura estándar para las respuestas de las APIs del sistema.
    /// Envuelve el resultado de la operación, el estado HTTP y los posibles mensajes de error o éxito.
    /// </summary>
    /// <typeparam name="T">El tipo del objeto de resultado devuelto en la respuesta.</typeparam>
    public class APIResponseDTO<T>
    {
        /// <summary>
        /// Código de estado HTTP de la respuesta.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Indica si la operación se procesó con éxito.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Lista de mensajes informativos, de advertencia o de error generados durante la operación.
        /// </summary>
        public List<string>? Messages { get; set; }

        /// <summary>
        /// El resultado de la operación en caso de éxito.
        /// </summary>
        public T? Result { get; set; }
    }
}
