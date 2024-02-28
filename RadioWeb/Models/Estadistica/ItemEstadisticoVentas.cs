using RadioWeb.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RadioWeb.Models.Estadistica
{
    public class ItemEstadisticoVentas
    {
        public string Anyo { get; set; }

        public string Mes { get; set; }
        public string Grupo { get; set; }
        public float Cuenta { get; set; }
        public float Cuenta2 { get; set; }
        public int DiferenciaCuenta { get; set; }
        public double DiferenciaCuentaPorc { get; set; }
        public double Ventas { get; set; }
        public double Venta2 { get; set; }
        public double DiferenciaVentas { get; set; }
        public double DiferenciaVentasPorc { get; set; }
        public double Media { get; set; }
        public double Dia { get; set; }
        public string Simbolo { get; set; }


    }
    public class ItemResumen
    {
        public string Anyo { get; set; }
        public string Mes { get; set; }
        public string DiaFormateado { get; set; }
        public int MesNumero { get; set; }
        public string Grupo { get; set; }
        public int Cuenta { get; set; }
        public int Cuenta2 { get; set; }
        public decimal DiferenciaCuenta { get; set; }
        public decimal DiferenciaCuentaPorc { get; set; }
        public decimal Ventas { get; set; }
        public decimal PromedioVentas { get; set; }
        public decimal Venta2 { get; set; }
        public decimal PromedioVentas2 { get; set; }

        public decimal VentaTotal { get; set; }
        public decimal VentaConsum { get; set; }
        public decimal Venta2Consum { get; set; }
        public decimal Venta2Total { get; set; }
        public decimal DiferenciaVentas { get; set; }
        public decimal DiferenciaVentasPorc { get; set; }
        public decimal Media { get; set; }
        public decimal Dia { get; set; }
    }

    public class ItemFacturasMes
    {
        
        public int Orden { get; set; }
        public string Mutua { get; set; }
        public string MutuaNombre { get; set; }
        public decimal Ventas { get; set; }
        public decimal Venta2 { get; set; }
        public decimal Enero { get; set; }
        public decimal Febrero { get; set; }
        public decimal Marzo { get; set; }
        public decimal Abril { get; set; }
        public decimal Mayo { get; set; }
        public decimal Junio { get; set; }
        public decimal Julio { get; set; }
        public decimal Agosto { get; set; }
        public decimal Septiembre { get; set; }
        public decimal Octubre { get; set; }
        public decimal Noviembre { get; set; }
        public decimal Diciembre { get; set; }        
        public decimal Total { get; set; }
      

    }
    //public class ItemResumenAcumulado
    //{
    //    public string Anyo { get; set; }


    //    public string Grupo { get; set; }
    //    public int Cuenta { get; set; }
    //    public int Cuenta2 { get; set; }
    //    public float DiferenciaCuenta { get; set; }
    //    public float DiferenciaCuentaPorc { get; set; }
    //    public float Ventas { get; set; }
    //    public float Venta2 { get; set; }
    //    public float DiferenciaVentas { get; set; }
    //    public float DiferenciaVentasPorc { get; set; }
    //    public float Media { get; set; }
    //    public float Dia { get; set; }
    //}
}
