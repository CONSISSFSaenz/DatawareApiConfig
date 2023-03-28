using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Consiss.DataWare.Functions.Utilities
{
    public static class Messages
    {
        /// <summary>
        /// Mensaje de error generico
        /// </summary>
        public const string SaveError = "Ocurrio un error al guardar los datos.";
        /// <summary>
        /// Mensaje de errro al guardar datos
        /// </summary>
        public const string UpdateError = "Ocurrio un error al intentar modificar los datos.";
        /// <summary>
        /// Mensaje de error - login
        /// </summary>
        public const string LoginError = "Usuario o contraseña no válida.";
        /// <summary>
        /// Mensaje de errro al Eliminar datos
        /// </summary>
        public const string DeleteError = "Ocurrio un error al intentar eliminar el dato.";
        /// <summary>
        /// Mensaje de errro al cargar todos los datos
        /// </summary>
        public const string ListError = "Ocurrio un error al listar los datos.";
        /// <summary>
        /// Mensaje de errro al cargar todos los datos
        /// </summary>
        public const string GetDataError = "Ocurrio un error al intentar obtener el dato.";

        public const string Error = "Error al sincronizar datos de Intelimotor";

        public const string SuccessMsg = "La sincronización se ha realizado con éxito";
    }
}
