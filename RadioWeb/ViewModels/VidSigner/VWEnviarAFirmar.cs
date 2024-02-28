using RadioWeb.Models;
using RadioWeb.Models.VidSigner;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RadioWeb.ViewModels.VidSigner
{
    public class VWEnviarAFirmar
    {

        public VWEnviarAFirmar()
        {
            ESDOCUMENTO = false;
        }
        [Required]
        public int OIDEXPLORACION { get; set; }
        public int IOR_PACIENTE { get; set; }
        public List<TABLETAS> Devices { get; set; }
        public List<P_INFORMES> Documents { get; set; }

        [Required]
        public string DNI { get; set; }
        public string DNIRESPOSABLE { get; set; }
        public string RESPONSABLE { get; set; }

        [Required]
        public string Accion { get; set; }

        [Required]
        public string DeviceSelected { get; set; }

        [Required]
        public int DocumentSelected { get; set; }
        

        public string NOMBRE { get; set; }
        public string EDAD { get; set; }

        public string LOPDFIRMADA { get; set; }
        public bool ESDOCUMENTO { get; set; }
    }
}