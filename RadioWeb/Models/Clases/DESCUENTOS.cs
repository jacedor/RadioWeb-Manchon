//------------------------------------------------------------------------------
// <auto-generated>
//    Este código se generó a partir de una plantilla.
//
//    Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//    Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RadioWeb.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class DESCUENTOS
    {
        [Key]
        public int OID { get; set; }
        public Nullable<int> VERS { get; set; }
        public string DESCRIPCION { get; set; }
        public int DESCUENTO { get; set; }
        public string CODIGO { get; set; }
    }
}
