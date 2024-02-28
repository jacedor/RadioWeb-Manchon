using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RadioWeb.Models
{

    public class LISTADIAAMBFORATS
    {
        public bool ANULADA { get; set; }
        public bool APLAZADO { get; set; }
        public bool ANULACONSENTIMIENTO{ get; set; }
        public int CANCELADO { get; set; }
        public double CANTIDAD { get; set; }
        public int CENTRO { get; set; }
        public Nullable<int> CID { get; set; }
        //color que se muestra en la casilla de la lista
        public string CIDCOLOR { get; set; }        
        public string COLOR_HORARIO { get; set; }
        public string COLOR { get; set; }
        public string COD_MED { get; set; }
        public string COD_MUT { get; set; }
        public string DES_MUT { get; set; }
        public string COD_FIL { get; set; }
        public string DES_FIL { get; set; }
        //SI ESTA HECHA, EN ESPERA, SI ESTA BORRADO
        public string ESTADO { get; set; }
        public Nullable<int> EDAD { get; set; }
        public string ESTADODESC { get; set; }
        public bool ESHUECO { get; set; }
        public string CLASEESPECIAL { get; set; }
        public DateTime FECHA { get; set; }
        public DateTime? FECHAMAXENTREGA { get; set; }
        public bool FACTURADA { get; set; }
        public string FIL { get; set; }
        public int GRUPOAPA { get; set; }
        public bool HAYCOMEN { get; set; }
        public bool HAYCONSUMIBLE { get; set; }
        //Hora del horario AQUI ES DONDE SE APLICA EL COLOR, SI ESTA LIBRE SE APLICA A TODA LA FILA SINO SE QUEDA EN LA COLUMNA
        public string HHORA { get; set; }
        public int IOR_PACIENTE { get; set; }
        public int IOR_GPR { get; set; }  
        public int OID { get; set; }
        public string HORA_LL { get; set; }
        public string HORA { get; set; }
        public string RUTA_CONSEN { get; set; }
        public string RUTA_LOPD { get; set; }
        public string HORA_EX { get; set; }
        public bool INFORMADA { get; set; }
        public bool QRCOMPARTIRCASO { get; set; }
        public int IOR_MASTER { get; set; }
        public bool FIRMA_CONSEN { get; set; }
        public bool LOPD { get; set; }
        public bool INTOCABLE { get; set; }
        //campo calculado QUE ES LA HORA ACTUAL MENOS LA HORA DE LLEGADA, SI LA HORA_EX ESTA LLENA LA DIFERENCIA CON ESTA
        public string ESPERA { get; set; }        
        public string PACIENTE { get; set; }
        public bool PAGADO { get; set; }
        public string MEDICO { get; set; }
        public string NUMEROS { get; set; }
        public string TEXTO { get; set; }
        public string TECNICO { get; set; }        
        //NO ENSEÑAR
        public int PRIVADO { get; set; }
        //NO ENSEÑAR       
        public string SIMBOLO { get; set; }       
        public bool NOFACTURAB { get; set; }
        //CAMPOS QUE NO ESTAN EN LISTADIA
        public int ORDER_HHORA { get; set; }
        public int ORDER_HORA { get; set; }
        public bool VIP { get; set; }
        public bool ACTIVA { get; set; }
        public Nullable<int> VERS { get; set; }     
        public string SUBTEXTO { get; set; }

        public int IOR_APARATO { get; set; }
        public PACIENTE PACIENTEOBJECT { get; set; }

        public string TEXTOAGENDA { get; set; }
    }
}