﻿//------------------------------------------------------------------------------
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

    public partial class VMMutuasCitaOnline
    {

        public VMMutuasCitaOnline()
        {
            this.DIRECCIONES = new HashSet<DIRECCION>();
            this.TELEFONOS = new HashSet<TELEFONO>();

        }

        [Key]
        public int OID { get; set; }

        [Display(Name = "TIPO")]
        [DataType("MUTUATIPO")]
        public int OWNER { get; set; }
        public Nullable<int> IOR_EMPRESA { get; set; }
        public string NOMBRE { get; set; }
        public string CODMUT { get; set; }

        [Display(Name = "CENTRAL")]
        [DataType("MUTUASLIST")]
        public Nullable<int> IOR_CENTRAL { get; set; }

        [UIHint("DIRECCION")]
        public virtual ICollection<DIRECCION> DIRECCIONES { get; set; }

        [UIHint("TELEFONO")]
        public virtual ICollection<TELEFONO> TELEFONOS { get; set; }

        public bool mutuaOnlineDisponible { get; set; }
    }
}