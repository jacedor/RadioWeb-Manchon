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

    public partial class CONSUMIBLES
    {
        [Key]
        public int OID { get; set; }
        public Nullable<int> VERS { get; set; }
        public Nullable<int> CID { get; set; }
        public string CANAL { get; set; }
        public Nullable<int> OWNER { get; set; }
        public string USERNAME { get; set; }
        public Nullable<System.DateTime> MODIF { get; set; }
        public Nullable<int> IOR_EMPRESA { get; set; }

        [Display(Name = "CÓDIGO")]
        [StringLength(6)]
        public string COD_CONSUM { get; set; }

        [Display(Name = "CONSUMIBLE")]
        [StringLength(80)]
        public string DES_CONSUM { get; set; }

        public Nullable<int> TOT_CONSUM { get; set; }
        public string BORRADO { get; set; }

        [NotMapped]
        public int[] SELECTED_GROUPS { get; set; }

        //public int IOR_EXPLORACION { get; set; }
        //public int IOR_ENTIDADPAGADORA { get; set; }
        //public string COD_MUT { get; set; }
        //public double PRECIO { get; set; }
        //public string PAGADO { get; set; }
        //public string APLAZADO { get; set; }
        //public int OID { get; set; }




    }
}
