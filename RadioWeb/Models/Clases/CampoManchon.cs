using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadioWeb.Models
{
    /// <summary>
    /// Clase que hereda de attribute y que usaremos para definir por qué campos buscar. 
    /// </summary>
    class CampoManchonAttribute : Attribute
    {

        /// <summary>
        /// Attribute constructor.
        /// </summary>
        public CampoManchonAttribute()
         {      
         }
        /// <summary>
        /// Este campo aparecerá en las combo de búsquedas??
        /// </summary>
        public bool Busqueda { get; set; }

        public bool GetValorBusqueda()
        {
            return Busqueda;
        }
    }
}
