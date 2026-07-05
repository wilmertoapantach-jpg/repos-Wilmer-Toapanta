using Microsoft.AspNetCore.Mvc;
using System.Net;
using workItem.DTO;
using workItem.Service.IService;
using workItem.Shared;

namespace workItem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkItemController(IWorkItemService workItemService) : ControllerBase
    {
        private readonly IWorkItemService _workItemService = workItemService;

        /// <summary>
        /// Guarda un nuevo elemento de trabajo o actualiza uno existente.
        /// Si es creación (WorkItemId == 0), el elemento se asigna automáticamente al usuario más apropiado.
        /// Si es actualización (WorkItemId > 0), solo se actualiza la información del elemento.
        /// </summary>
        /// <param name="request">Objeto con los datos del elemento de trabajo a crear o actualizar</param>
        /// <returns>Respuesta con el elemento de trabajo guardado o actualizado</returns>
        [HttpPost("SaveWorkItem")]
        public async Task<ActionResult<APIResponseDTO<WorkItemResponseDTO>>> SaveWorkItem([FromBody] WorkItemRequestDTO request)
        {
            try
            {
                var result = await _workItemService.SaveWorkItem(request);
                return Ok(new APIResponseDTO<WorkItemResponseDTO>
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponseDTO<WorkItemResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Messages = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Obtiene un listado de elementos de trabajo con filtros opcionales y paginación.
        /// Permite filtrar por ID, estado, relevancia y usuario asignado.
        /// </summary>
        /// <param name="filter">Criterios de filtrado y parámetros de paginación</param>
        /// <returns>Página con elementos de trabajo que coinciden con los filtros</returns>
        [HttpPost("ListWorkItems")]
        public async Task<ActionResult<APIResponseDTO<PageResponseDTO<WorkItemResponseDTO>>>> ListWorkItems([FromBody] WorkItemFilterDTO filter)
        {
            try
            {
                var result = await _workItemService.ListWorkItems(filter);
                return Ok(new APIResponseDTO<PageResponseDTO<WorkItemResponseDTO>>
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponseDTO<PageResponseDTO<WorkItemResponseDTO>>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Messages = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Asigna o reasigna un elemento de trabajo existente al usuario más apropiado
        /// usando las reglas de distribución de carga.
        /// </summary>
        /// <param name="workItemId">ID del elemento de trabajo a asignar</param>
        /// <returns>Respuesta con el elemento de trabajo asignado actualizado</returns>
        [HttpPost("AssignWorkItem")]
        public async Task<ActionResult<APIResponseDTO<WorkItemResponseDTO>>> AssignWorkItem([FromBody] int workItemId)
        {
            try
            {
                var result = await _workItemService.AssignWorkItem(workItemId);
                return Ok(new APIResponseDTO<WorkItemResponseDTO>
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponseDTO<WorkItemResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Messages = new List<string> { ex.Message }
                });
            }
        }
    }
}
