using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RadioWeb.DTO
{
    public class PacienteQreport
    {
        public int oid { get; set; }
        public string nombre{ get; set; }       
        public string email { get; set; }
        public string dni { get; set; }
        public string telefono { get; set; }
        public string codmut { get; set; }
        public string descmut { get; set; }
        public string ior_colegiado { get; set; }
        public string medico_referidor { get; set; }
        public string ior_tipoexploracion { get; set; }
        public string descexploracion { get; set; }
        public string ior_especialidad { get; set; }
        public string descespecialidad { get; set; }
        public string qrcompartircaso { get; set; }
        public string qrenlaceDirecto { get; set; }
        
    }
}