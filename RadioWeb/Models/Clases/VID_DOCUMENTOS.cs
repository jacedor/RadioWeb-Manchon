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
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("VID_DOCUMENTOS")]
    public partial class VID_DOCUMENTOS
    {
        [Key]
        public int OID { get; set; }
        public Nullable<int> VERS { get; set; }
        public Nullable<int> CID { get; set; }   
        public Nullable<int> OWNER { get; set; }
        public Nullable<int> IOR_PACIENTE { get; set; }
        public string NOMBRE { get; set; }
        public Nullable<int> CANAL { get; set; }
        public string BORRADO { get; set; }
        public string DOCGUI { get; set; }

        public string FECHA { get; set; }
        
       
    }
}