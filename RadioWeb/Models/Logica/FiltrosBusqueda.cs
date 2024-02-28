using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RadioWeb.Models.Logica
{
    public class DatosNavegacion {
        public bool filtroBorrados { get; set; }
        public string  ultimaUrl { get; set; }    
    }


    public class FiltrosBusquedaExploracion
    {        
        public int oidAparato { get; set; }
        public string DescAparato{ get; set; }
        public int oidExploracion { get; set; }
        public string DescExploracion { get; set; }  
        public int oidGrupoAparato { get; set; }
        public int oidMutua { get; set; }
        public List<int> MutuaList { get; set; }
        public int oidEstadoExploracion { get; set; }
        public string DescMutua { get; set; }
        public int oidCentro { get; set; }
        public string Fecha { get; set; }
        public string Hora{ get; set; }
        public string Paciente { get; set; }
        public string Borrados { get; set; }
        public string SoloHuecos { get; set; }
        public string OrderField { get; set; }
        public string OrderDirection { get; set; }
        public string Comentario { get; set; }
        
        public int IOR_COLEGIADO { get; set; }
        public int oidMedicoInformante { get; set; }
        public int oidExploracionSeleccionada { get; set; }
        public string modoAgendaMultiple { get; set; }
        public string pagado { get; set; }
        public string facturado { get; set; }
        public string informada { get; set; }
        public string busquedaTotal { get; set; }
        public string busquedaTotalPorMedico { get; set; }
        public int iorPaciente { get; set; }
        //propiedad usada en el resumen para comparar con el anyo actual menos el numero de esta propiedad
        public int anyo { get; set; }
        public  FiltrosBusquedaExploracion()
        {
            this.modoAgendaMultiple = "mismoAparato";
        }
    }

    public class FiltrosBusquedaMultiple
    {
        public int oidAparato { get; set; }        
        public string Fecha { get; set; }
        public string Hora { get; set; }
        public string Paciente { get; set; }
        public string Borrados { get; set; }
        public string Comentario { get; set; }
        public List<int> oidExploracionSeleccionada { get; set; }
    }

    public class FiltrosBusquedaPaciente
    {

        public int oid { get; set; }
        public string Nombre { get; set; }
        public string Dni { get; set; }
        public string Telefono { get; set; }
        public string Status { get; set; }
        

    }
}