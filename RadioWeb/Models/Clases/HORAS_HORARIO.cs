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
    
    public partial class HORAS_HORARIO
    {
        public int OID { get; set; }
         [System.ComponentModel.DefaultValue(true)] 
        public bool ANULADA { get; set; }
        public Nullable<int> VERS { get; set; }
        public Nullable<int> CID { get; set; }
        public Nullable<int> IOR_DAPA { get; set; }
        public Nullable<int> IOR_FECHAHORARIO { get; set; }
        public Nullable<short> NDIA { get; set; }
        public DateTime FECHA { get; set; }
        public string HORA { get; set; }
        public string COLOR { get; set; }
        public string TEXTODEFECTO { get; set; }
        public string USERNAME { get; set; }
        public Nullable<System.DateTime> MODIF { get; set; }
        public string BORRADO { get; set; }
    }
}