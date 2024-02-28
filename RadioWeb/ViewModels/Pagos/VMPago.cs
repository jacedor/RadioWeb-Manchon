using RadioWeb.Models;
using RadioWeb.Models.Repos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.ViewModels.Pagos
{
    public class VMPago
    {


        public VMPago()
        {

            this.MUTUAS = MutuasRepositorio.Lista(false);
    
        }

        public double CANTIDADTOTALCONSUMIBLES { get; set; }

        public double CANTIDADPAGADA { get; set; }

        public double CANTIDAD { get; set; }


        public double CANTIDADPENDIENTE { get; set; }

        public double TOTAL { get; set; }

        public List<EXPLORACION> EXPLORACIONES { get; set; }
        public double CANTIDADCONSUPENDIENTE { get; internal set; }

        [DisplayName("PAGADOR")]
        public int IOR_MUTUACONSUMIBLE { get; set; }
        public List<MUTUAS> MUTUAS { get; set; }

        public int IOR_GAPARATO { get; set; }
        public string GRUPO { get; set; }

        public int OIDEXPLORACION { get; set; }
        

        [DisplayName("CONSUMIBLE")]
        public int IOR_ADDCONSUMIBLE { get; set; }
        public List<PRECIOS_CONSUM> CONSUMIBLESASIGNABLES { get; set; }
        public MONEDAS MONEDA { get; set; }

    }

}