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

    [Table("AYUDA")]
    public partial class AYUDA
    {
        [Key]
        public int OID { get; set; }
        public Nullable<int> VERS { get; set; }
        public Nullable<int> CID { get; set; }
        public string CANAL{ get; set; }
        public string USERNAME { get; set; }
        public DateTime MODIF { get; set; }  
        public Nullable<int> IOR_EMPRESA { get; set; }
        public string BORRADO { get; set; }
        public string TITULO { get; set; }
        public string TAGS { get; set; }
        public int VISITADO { get; set; }   
        
        [NotMapped]
        public string TEXTO { get; set; }
    }
}