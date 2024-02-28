using RadioWeb.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RadioWeb.ViewModels
{
    public class VWAgendaGen
    {

        public VWAgendaGen()
        {
            
        }
        public int OID { get; set; }

        [Required]
        [DisplayName("Fecha")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public System.DateTime  AGENDA { get; set; }

        public string fechaInicio{ get; set; }
        public string fechaFin { get; set; }

        [Required]
        [DisplayName("Texto")]
        public string TEXTO { get; set; }

       
        public int IOR_APARATO { get; set; }
        

        public List<DAPARATOS> APARATOS { get; set; }

        public List<DAPARATOS> APARATOSSELECCIONADOS { get; set; }

        public string ACTION
        {
            get
            {
                return (OID != 0) ? "Update" : "Create";
            }

        }



        //Campo de logica de negocio que se usa para saber si estamos asociando una exploración a dicho colegiado
        public int OIDEXPLORACION { get; set; }
    }

   
}