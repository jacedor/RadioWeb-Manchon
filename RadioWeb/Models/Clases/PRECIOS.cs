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
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public partial class PRECIOS
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
        public string BORRADO { get; set; }
        public Nullable<double> CODIGO { get; set; }
        public string COD_MUT { get; set; }
        public string COD_FIL { get; set; }
        public Nullable<double> CANTIDAD { get; set; }
        public Nullable<double> EURO_CANT { get; set; }
        public string EURO_P { get; set; }
        public string CODI_MUTUA { get; set; }
        [DataType("GAPARATO")]
        [DisplayName("GRUPO")]
        public Nullable<int> IOR_GAPARATO { get; set; }
        public int IOR_TIPOEXPLORACION { get; set; }

        [DataType("MUTUASLIST")]
        [DisplayName("MUTUA")]
        public Nullable<int> IOR_ENTIDADPAGADORA { get; set; }
        public Nullable<int> IOR_MONEDA { get; set; }

        //TIPO DE EXPLORACION
        public virtual APARATOS APARATO { get; set; }
    }
}