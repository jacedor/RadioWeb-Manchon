using RadioWeb.Models;
using RadioWeb.Models.Repos;
using RadioWeb.Utils;
using RadioWeb.ViewModels.Informes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RadioWeb.ViewModels.Exploracion
{
    public class VWBusquedaAvanzada
    {
        public VWBusquedaAvanzada()
        {
            this.CENTROS = CentrosRepositorio.List();
            //this.APARATOS = AparatoRepositorio.List();
            this.APARATOS = new List<Models.APARATOS>();
            this.GRUPOS = GAparatoRepositorio.Lista();
            this.DAPARATOS = DaparatoRepositorio.Lista();
            if (System.Web.HttpContext.Current.Application["Mutuas"] != null)
            {
                IEnumerable<MUTUAS> oListTemp = ((List<MUTUAS>)System.Web.HttpContext.Current.Application["Mutuas"]).Where(s => s.VERS == 1);
                this.MUTUAS = oListTemp.ToList();
            }
            else {
                this.MUTUAS = MutuasRepositorio.Lista();
            }
           
            this.MEDICOS = PersonalRepositorio.ObtenerMedicos();
            this.IOR_APARATO = -1;
            this.FECHAINICIAL = DateTime.Now.AddMonths(-1).ToString("dd/MM/yyyy");
            this.FECHAFINAL = DateTime.Now.ToString("dd/MM/yyyy");
            this.PendienteRevisa = false;
            this.Resultados = new List<VMExploNoInformadas>();
        }

       


        [DisplayName("Fecha Inicial")]
        [Required]
        public string FECHAINICIAL{ get; set; }

        [DisplayName("Fecha Final")]
        [Required]
        public string FECHAFINAL { get; set; }

        [DisplayName("Fecha Fac Inicial")]    
        public string FECHAFACINICIAL { get; set; }

        [DisplayName("Fecha Fac Final")]  
        public string FECHAFACFINAL { get; set; }

        [DisplayName("Buscar pendientes Revisar")]
        public bool PendienteRevisa { get; set; } 

        public List<CENTROS> CENTROS { get; set; }

        [DisplayName("Centro")]
        public int IOR_CENTRO { get; set; }

        public List< GAPARATOS> GRUPOS { get; set; }

        [DisplayName("Grupo")]
        public int  IOR_GAPARATO{ get; set; }

   
        public List<DAPARATOS> DAPARATOS { get; set; }

        [DisplayName("Aparato")]
        public int IOR_DAPARATO { get; set; }


        public List<APARATOS> APARATOS { get; set; }

        [DisplayName("Exploracion")]
        public int IOR_APARATO { get; set; }

        public int IOR_TIPO { get; set; }

        [DisplayName("Qreport")]
        public bool QRCOMPARTIRCASO { get; set; }

        public List<MUTUAS> MUTUAS { get; set; }

        [DisplayName("TARIFA/MUTUA")]
        [DataType("MUTUASLIST")]
        public int IOR_ENTIDADPAGADORA { get; set; }

        [DisplayName("FECHA INFORME")]
        public string FECHAMAXENTREGA { get; set; }        



        [DisplayName("MÉDICO INFORME")]
        public int IOR_MEDICOINFORMANTE { get; set; }

        public string NOMBREMEDICOINFORMANTE { get; set; }

        [DisplayName("MÉDICO REVISION")]
        public int IOR_MEDICOREVISION{ get; set; }

        public string NOMBREMEDICOREVISION { get; set; }

        [DisplayName("CARDIOLOGO")]
        public int IOR_CARDIOLOGO { get; set; }

        public List<PERSONAL> CARDIOLOGOS { get; set; }

        public List<PERSONAL> MEDICOS { get; set; }

        [DisplayName("MÉDICO REFERIDOR")]
        public int IOR_MEDICOREFERIDOR { get; set; }

        public string NOMBREMEDICOREFERIDOR { get; set; }

        public List<COLEGIADOS> MEDICOSREFERIDORES { get; set; }

        public string INFORMADA { get; set; }

        public int IOR_INFORME { get; set; }


        public List<VMExploNoInformadas> Resultados { get; set; }



    }
}