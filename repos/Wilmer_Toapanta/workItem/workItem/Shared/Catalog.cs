namespace workItem.Shared
{
    public class Catalog
    {

        // ─── Constantes para la lógica de distribución ────────────────────────────────
        /// <summary>Identificador de relevancia alta para elementos de trabajo</summary>
        public const int HIGH_RELEVANCE = 1;

        /// <summary>Identificador de relevancia baja para elementos de trabajo</summary>
        public const int LOW_RELEVANCE = 0;

        /// <summary>Umbral máximo de tareas de alta relevancia asignadas a un usuario antes de considerarlo saturado</summary>
        public const int SATURATION_THRESHOLD = 3;   // >3 elementos de alta relevancia → saturado

        /// <summary>Días desde hoy para considerar una tarea como próxima a vencer</summary>
        public const int NEAR_DUE_DAYS = 3;           // <3 días desde ahora → próxima a vencer

        public const int STATUS_ACTIVE = 1;           // Estado activo

    }
}
