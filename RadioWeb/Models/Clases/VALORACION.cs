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

    [Table("VALORACION")]
    public partial class VALORACION
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
        public Nullable<System.DateTime> FECHA { get; set; }
        public Nullable<double> VISITA { get; set; }
        public string HORAMOD { get; set; }
        public Nullable<int> IOR_PAC { get; set; }
        public Nullable<int> IOR_MEDINFORME { get; set; }
        public Nullable<int> IOR_MEDREVISION { get; set; }
        public string PATOLOGICO { get; set; }
        public Nullable<int> IOR_TECNICO { get; set; }
        public Nullable<int> IOR_MODALIDAD { get; set; }
        public Nullable<int> IOR_SITUACION { get; set; }
        public Nullable<int> IOR_TECNICA { get; set; }
        public string V_RADIOLOGO { get; set; }
        public string V_IMAGEN { get; set; }
        public string TEXTO_RADIOLOGO { get; set; }
        public string TEXTO_IMAGEN { get; set; }
        public string ERROR { get; set; }
        public string COMITE_OF { get; set; }
        public string COMITE_IM { get; set; }
        public string COMITE_OF_COMEN { get; set; }
        public string COMITE_IM_COMEN { get; set; }
        public Nullable<System.DateTime> FECHA_VALORACION { get; set; }
        public Nullable<System.DateTime> FECHA_COMITE_IMAGEN { get; set; }
        public Nullable<System.DateTime> FECHA_COMITE_RADIOLOGO { get; set; }
    }
}